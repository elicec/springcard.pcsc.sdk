/**h* SpringCardNFC/RtdMedia
 *
 * NAME
 *   SpringCard API for NFC Forum :: RTD Media class
 * 
 * COPYRIGHT
 *   Copyright (c) SpringCard SAS, 2012
 *   See LICENSE.TXT for information
 *
 **/
using System;
using SpringCardPCSC;

namespace SpringCardNFC
{
	/**c* SpringCardNFC/RtdMedia
	 *
	 * NAME
	 *   RtdMedia
	 * 
	 * DESCRIPTION
	 *   Represents a Ndef Record that stores an arbitrary MIME Media
	 *
	 * SYNOPSIS
	 *   RtdMedia media = new RtdMedia(string MimeType)
	 *   RtdMedia media = new RtdMedia(string MimeType, string TextContent)
	 *   RtdMedia media = new RtdMedia(string MimeType, byte[] RawContent)
	 * 
	 * DERIVED FROM
	 *   Rtd
	 * 
	 * DERIVED BY
	 *   RtdVCard
	 *
	 **/
	public class RtdMedia : Rtd
	{
		public RtdMedia(string MimeType) : base(NDEF_HEADER_TNF_MEDIA_TYPE, MimeType)
		{
			
		}
		
		public RtdMedia(string MimeType, string TextContent) : base(NDEF_HEADER_TNF_MEDIA_TYPE, MimeType)
		{
			_payload = CardBuffer.BytesFromString(TextContent);
		}

		public RtdMedia(string MimeType, byte[] RawContent) : base(NDEF_HEADER_TNF_MEDIA_TYPE, MimeType)
		{
			_payload = RawContent;
		}
		
		/**v* SpringCardNFC/RtdMedia.TextContent
		 *
		 * SYNOPSIS
		 *   public string TextContent
		 * 
		 * DESCRIPTION
		 *  Returns the payload of the RtdMedia object, as an ASCII string
		 *
		 **/
		public string TextContent
		{
			get
			{
				if (_payload == null)
					return "";
				return CardBuffer.StringFromBytes(_payload);
			}
		}
		
		/**v* SpringCardNFC/RtdMedia.RawContent
		 *
		 * SYNOPSIS
		 *   public byte[] RawContent
		 * 
		 * DESCRIPTION
		 *  Returns the payload of the RtdMedia object, as a byte array
		 *
		 **/
		public byte[] RawContent
		{
			get
			{
				return _payload;
			}
		}

	}
	
}
