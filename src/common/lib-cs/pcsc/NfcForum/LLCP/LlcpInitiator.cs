﻿/*
 * Crée par SharpDevelop.
 * Utilisateur: johann
 * Date: 05/01/2012
 * Heure: 12:03
 * 
 * Pour changer ce modèle utiliser Outils | Options | Codage | Editer les en-têtes standards.
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;
using SpringCardPCSC;

namespace SpringCardNFC
{
	public class LlcpInitiator : LLCP
	{
		private Thread initiator_thread = null;
		private bool thread_running;
		private bool reset_field;
		private bool suspend_reader;
		
		public LlcpInitiator(string ReaderName) : base(ReaderName)
		{
			
		}
		
		public LlcpInitiator(SCardReader Reader) : base (Reader)
		{
			
		}
		
		public LlcpInitiator(SCardChannel Channel) : base (Channel)
		{
			
		}
		
		public override bool Start()
		{
			if (!_channel.Connect())
				return false;
			
			OnLinkEstablished();
			
			reset_field = false;
			suspend_reader = false;
			thread_running = true;
			initiator_thread = new Thread(thread_proc);
			initiator_thread.Start();
			
			return true;
		}
		
		public override void Stop()
		{
			reset_field = false;
			suspend_reader = false;
			thread_running = false;
		}

		public override void StopAndResetPeer()
		{
			reset_field = true;
			suspend_reader = false;
			thread_running = false;
		}

		public override void StopAndSuspend()
		{
			reset_field = true;
			suspend_reader = true;
			thread_running = false;
		}
		
		protected override LLCP_PDU Exchange(LLCP_PDU send_pdu)
		{
			Trace.WriteLine("< " + send_pdu.AsString());
			
			CAPDU capdu = new CAPDU(0xFF, 0xFE, 0x00, 0x00, send_pdu.GetBytes());
			
			Trace.WriteLine("< " + capdu.AsString());
			
			RAPDU rapdu = _channel.Transmit(capdu);
			
			if (rapdu == null)
				return null;

			Trace.WriteLine("> " + rapdu.AsString());
			
			if (rapdu.SW != 0x9000)
				return null;
			
			LLCP_PDU recv_pdu = new LLCP_PDU(rapdu.data);
			
			Trace.WriteLine("> " + recv_pdu.AsString());
			
			return recv_pdu;
		}
		
		private void thread_proc()
		{
			Trace.WriteLine("LLCP Initiator starting...");
			
			while (thread_running)
			{
				LLCP_PDU send_pdu = SendPDU_POP();
				
				if (send_pdu == null)
				{
					/* We send a SYMM PDU */
					send_pdu = new LLCP_SYMM_PDU();
				}
				
				LLCP_PDU recv_pdu = Exchange(send_pdu);
				
				if (recv_pdu == null)
				{
					Trace.WriteLine("Exchange failed");
					break;
				}
				
				if ((recv_pdu.PTYPE == LLCP_PDU.PTYPE_SYMM) && (recv_pdu.DSAP == 0) && (recv_pdu.SSAP == 0))
				{
					/* SYMM */
				}
				
				if (!HandleRecvPDU(recv_pdu))
				{
					Trace.WriteLine("Process failed");
					break;
				}
			}
			
			Trace.WriteLine("LLCP Initiator exiting");
			
			if (reset_field)
			{
				Trace.WriteLine("Reset the RF field");
				_channel.Transmit(new CAPDU(0xFF, 0xFB, 0x10, 0x00));
				System.Threading.Thread.Sleep(300);
//				_channel.Transmit(new CAPDU(0xFF, 0xFB, 0x10, 0x01));
//				System.Threading.Thread.Sleep(300);
				reset_field = false;
			}
			
			if (suspend_reader)
			{
				Trace.WriteLine("Stop the reader");
				_channel.Transmit(new CAPDU(0xFF, 0xF0, 0x00, 0x00, new byte[] { 0x22 }));
				suspend_reader = false;
			}
			
			Trace.WriteLine("Disconnect...");
			_channel.DisconnectEject();
			Trace.WriteLine("LLCP Initiator terminated");
		}
		
		public static bool SetServiceList(SCardReader reader, ushort servers)
		{
			Trace.WriteLine("Sending ATR_GI to the reader '" + reader.Name + "'");
			
			byte[] atrGi = new byte[13];
			
			/* NFC-Forum magic number for LLCP */
			atrGi[0] = 0x46; atrGi[1] = 0x66; atrGi[2] = 0x6D;
			
			/* LLCP version 1.1 */
			atrGi[3] = 0x01; atrGi[4] = 0x01; atrGi[5] = 0x11;
			
			/* Service listeners: 0x0001 for Service Discovery, 0x0010 for SNEP */
			atrGi[6] = 0x03; atrGi[7] = 0x02;
			atrGi[8] = (byte) (servers / 0x0100);
			atrGi[9] = (byte) (servers % 0x0100);
			
			/* Link timeout: 1.5s */
			atrGi[10] = 0x04; atrGi[11] = 0x01; atrGi[12] = 0x96;
			
			Trace.WriteLine("ATR_GI=" + (new CardBuffer(atrGi)).AsString());
			
			/* Assemble the control command */
			byte[] control_command = new byte[3 + atrGi.Length];
			
			control_command[0] = 0x58; /* SpringProx control header */
			control_command[1] = 0x8D; /* Push register */
			control_command[2] = 0xE1; /* Register = 0xE1 for ATR_GI */
			for (int i=0; i<atrGi.Length; i++)
				control_command[3 + i] = atrGi[i];
			
			/* Send the new ATR_GI to the reader */
			return ControlReader(reader, control_command);
		}
		
		public static bool AnnounceSnepServer(SCardReader reader, bool yes)
		{
			ushort services_list = 0x0001;
			if (yes)
				services_list |= 0x0002;
			
			/* Update the reader's ATR_GI */
			return LlcpInitiator.SetServiceList(reader, services_list);
		}

		public static bool Suspend(SCardReader reader)
		{
			byte[] control_command = new byte[2];
			
			control_command[0] = 0x58;
			control_command[1] = 0x22;
			
			return ControlReader(reader, control_command);
		}

		public static bool Resume(SCardReader reader)
		{
			byte[] control_command = new byte[2];
			
			control_command[0] = 0x58;
			control_command[1] = 0x23;
			
			return ControlReader(reader, control_command);
		}
		
		private static bool ControlReader(SCardReader reader, byte[] control_command)
		{
			SCardChannel card = new SCardChannel(reader);
			card.Protocol = 0;
			card.ShareMode = SCARD.SHARE_DIRECT;
			if (!card.Connect())
			{
				Trace.WriteLine("Failed to connect (DIRECT) to the reader");
				return false;
			}
			
			byte[] control_response = card.Control(control_command);
			if (control_response == null)
			{
				Trace.WriteLine("SCardControl failed");
				card.DisconnectLeave();
				return false;
			}
			
			card.DisconnectLeave();

			if (control_response.Length != 1)
			{
				Trace.WriteLine("SCardControl: bad response from reader");
				return false;
			}
			
			if (control_response[0] != 0)
			{
				Trace.WriteLine("SCardControl: error signaled by the reader: " + (0 - (int) control_response[0]));
				return false;
			}
			
			return true;
		}
		
	}
}
