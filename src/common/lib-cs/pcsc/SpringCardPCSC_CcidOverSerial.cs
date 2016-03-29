/**h* SpringCard/PCSC_CcidOver
 *
 **/
using System;
using System.IO.Ports;
using System.Diagnostics;
using System.Threading;
using SpringCard.LibCs;

namespace SpringCard.PCSC
{
	public class SCardReaderList_CcidOverSerial : SCardReaderList_CcidOver
	{
		bool running;
		SerialPort serialPort;
		byte[] recvBuffer;
		Thread receiverThread;
		Mutex receiverMutex;
		AutoResetEvent receiverEvent;
		
		public static SCardReaderList_CcidOverSerial Instantiate(string PortName, bool UseNotifications, bool UseLpcdPolling)
		{
			SCardReaderList_CcidOverSerial r = new SCardReaderList_CcidOverSerial();			
			
			Logger.Trace("OpenDevice " + PortName + "...");

			if (!r.OpenDevice(PortName))
				return null;
			
			Logger.Trace("MakeReaderList...");
			
			if (!r.MakeReaderList()) {
				r.CloseDevice();
				return null;
			}
			
			Logger.Trace("StartDevice...");
			
			if (!r.StartDevice(UseNotifications, UseLpcdPolling)) {
				r.CloseDevice();
				return null;
			}
			
			Logger.Trace("Device ready");
			return r;
		}
		
		public delegate void BackgroundInstantiateCallback(SCardReaderList_CcidOverSerial instance);
		private static Thread instantiateThread;
		private class InstantiateParams
		{
			public BackgroundInstantiateCallback Callback;
			public string PortName;
			public bool UseNotifications;
			public bool UseLpcdPolling;
		}
		public static void BackgroundInstantiate(BackgroundInstantiateCallback Callback, string PortName, bool UseNotifications, bool UseLpcdPolling)
		{
			if (Callback == null)
				return;
			
			InstantiateParams instantiateParams = new InstantiateParams();
			
			instantiateParams.Callback = Callback;
			instantiateParams.PortName = PortName;
			instantiateParams.UseNotifications = UseNotifications;
			instantiateParams.UseLpcdPolling = UseLpcdPolling;
			
			instantiateThread = new Thread(InstantiateProc);
			instantiateThread.Start(instantiateParams);
		}
		
		private static void InstantiateProc(object p)
		{
			InstantiateParams instantiateParams = (InstantiateParams)p;
			
			Logger.Trace("Background instantiate");
			
			SCardReaderList_CcidOverSerial instance = Instantiate(instantiateParams.PortName, instantiateParams.UseNotifications, instantiateParams.UseLpcdPolling);
			
			Logger.Trace("Calling the callback");
			
			instantiateParams.Callback(instance);
		}
				
		public SCardReaderList_CcidOverSerial()
		{
		}
		
		protected bool OpenDevice(string PortName)
		{
			Logger.Trace("Opening " + PortName);
			
			running = true;

			receiverMutex = new Mutex(false);
			receiverEvent = new AutoResetEvent(false);
			
			serialPort = new SerialPort(PortName, 38400, Parity.None, 8, StopBits.One);
			serialPort.Handshake = Handshake.None;
			serialPort.DataReceived += new SerialDataReceivedEventHandler(serialDataReceived);
			
			try {
				serialPort.Open();
			} catch (Exception e) {
				Logger.Trace(e.Message);
				running = false;
				return false;
			}
			
			if (!serialPort.IsOpen) {
				Logger.Trace("Failed to open " + serialPort.PortName);
				running = false;
				return false;
			}

			receiverThread = new Thread(ReceiverThread);
			receiverThread.Start();
			
			return true;		
		}
		
		protected override void CloseDevice()
		{
			if (receiverThread != null) {
				Logger.Trace("Stopping the receiver");
				running = false;
				receiverEvent.Set();
				receiverThread = null;
			}
			
			if (serialPort != null) {
				Logger.Trace("Closing " + serialPort.PortName);
				serialPort.Close();			
				serialPort = null;
			}
		}
		
		protected bool StartDevice(bool UseInterrupts = true, bool UseLowPowerCardDetect = false)
		{
			byte bOptions = 0;
			
			deviceState = DeviceState.NotActive;
			
			if (UseInterrupts)
				bOptions |= 0x01;
			if (UseLowPowerCardDetect)
				bOptions |= 0x02;			
			
			if (!sendControl(CCID.SET_CONFIGURATION, 0x0001, 0, bOptions))
				return false;			
			if (!WaitControl())
				return false;
			
			if (deviceState != DeviceState.Active)
				return false;
			
			return base.StartDevice();
		}
		
		protected override bool Send(byte endpoint, byte[] buffer)
		{
			Logger.Trace("\t" + BinConvert.ToHex(endpoint) + " < " + BinConvert.ToHex(buffer));
			
			byte[] t = new byte[3 + buffer.Length];
			
			t[0] = 0xCD;
			t[1] = endpoint;

			for (int i = 0; i < buffer.Length; i++)
				t[2 + i] = buffer[i];			
			
			byte crc = 0;
			for (int i = 1; i < t.Length - 1; i++)
				crc ^= t[i];
			t[t.Length - 1] = crc;
			
			Logger.Trace("<" + BinConvert.ToHex(t));
			
			try {
				serialPort.Write(t, 0, t.Length);
			} catch (Exception) {
				return false;
			}
			
			return true;
		}
		
		private void ReceiverThread()
		{			
			while (running) {
				receiverEvent.WaitOne();
				if (!running)
					break;
				
				receiverMutex.WaitOne();
				
				if ((recvBuffer != null) && (recvBuffer.Length > 0)) {
					if (recvBuffer[0] != 0xCD) {
						Logger.Trace("Cleanup:" + BinConvert.ToHex(recvBuffer));
						while ((recvBuffer != null) && (recvBuffer.Length > 0) && (recvBuffer[0] != 0xCD)) {
							recvBuffer = RawData.CopyBuffer(recvBuffer, 1);
						}
						if (recvBuffer != null) {
							Logger.Trace("Cleaned:" + BinConvert.ToHex(recvBuffer));
						}
					}
					
					if ((recvBuffer != null) && (recvBuffer.Length >= 12)) {
						ulong dataLength = (ulong)(recvBuffer[3] | (recvBuffer[4] << 8) | (recvBuffer[5] << 16) | (recvBuffer[6] << 24));
						
						if (dataLength > 262) {
							Logger.Trace("Length is invalid, discarding...");
							recvBuffer = null;
						} else if (recvBuffer.Length > (uint)(12 + dataLength)) {
							Logger.Trace(">" + BinConvert.ToHex(recvBuffer, (int)(12 + dataLength)));
							
							byte endpoint = recvBuffer[1];
							byte[] buffer = RawData.CopyBuffer(recvBuffer, 2, (int)(10 + dataLength));
							
							byte recv_crc = recvBuffer[12 + dataLength];								
							
							recvBuffer = RawData.CopyBuffer(recvBuffer, (int)(12 + dataLength));

							byte calc_crc = endpoint;
							
							for (int i = 0; i < buffer.Length; i++)
								calc_crc ^= buffer[i];
							
							if (calc_crc != recv_crc) {
								Logger.Trace("The CRC is incorrect");
							} else {								
								Recv(endpoint, buffer);
							}
															
							recvBuffer = RawData.CopyBuffer(recvBuffer, (int)(13 + dataLength));
							
							if (recvBuffer != null) {
								Logger.Trace("Remaining:" + BinConvert.ToHex(recvBuffer));
							}								
						
						} else {
							Logger.Trace("Buffer too short");
						}
					}
				}
				
				receiverMutex.ReleaseMutex();
			}
			
			Logger.Trace("Receiver exiting");
		}

		void serialDataReceived(object sender, SerialDataReceivedEventArgs e)
		{			
			try {
				int bytesReceived = serialPort.BytesToRead;
				byte[] buffer = new byte[bytesReceived];				
				serialPort.Read(buffer, 0, bytesReceived);
							
				receiverMutex.WaitOne();
				if (recvBuffer == null) {
					recvBuffer = buffer;					
				} else {
					byte[] t = new byte[recvBuffer.Length + buffer.Length];
					int i;
					for (i = 0; i < recvBuffer.Length; i++)
						t[i] = recvBuffer[i];
					for (i = 0; i < buffer.Length; i++)
						t[recvBuffer.Length + i] = buffer[i];
					recvBuffer = t;					
				}

				Logger.Trace(">>>" + BinConvert.ToHex(recvBuffer));

				receiverMutex.ReleaseMutex();
				receiverEvent.Set();				
			} catch (Exception ex) {
				Logger.Trace(ex.Message);
			}
		}
	}
}
