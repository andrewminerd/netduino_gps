using System;
using Microsoft.SPOT;

namespace NMEA.Examples
{
    class GPGGA
    {
        private NmeaParser parser = new NmeaParser();

		///<summary>
		/// Parses GPGGA sentences from the given buffer and returns true
		/// when a sentence is parsed that provides GPS fix information.
		/// If the buffer does not contain an entire sentence, the
		/// sentence is not valid (for instance, the checksum fails), or
		/// the sentence is syntactically correct but does not contain
		/// location information (i.e., the GPS device does not have a
		/// fix.)
		///</summary>
		///<param name="buff">Buffer to parse from</param>
		///<param name="offset">Offset in the buffer to begin parsing at</param>
		///<param name="len">Maximum number of bytes to read from the buffer</param>
		///<returns>True if a sentence was parsed that provided a fix, false otherwise</returns>
        public bool parse(byte[] buff, int offset, int len)
        {
            bool updated = false;
            for (int i = offset; i < len; i++)
            {
                if (parser.parse(buff[i]))
                {
                    Debug.Print(new String(System.Text.Encoding.UTF8.GetChars(parser.cmd), 0, parser.offset));

					// in the (unlikely?) event that the buffer contains a sentence
					// with a fix followed by a sentence without a fix, this would
					// return true while this.status = 0 and this.fix = false
                    updated = parseSentence(parser) || updated;
                }
            }
            return updated;
        }

        public int status;
        public int sat;
        public int speed;
        public long lat;
        public long lon;
        public int heading;
        public bool fix = false;

        private bool parseSentence(NmeaParser parser)
        {
			// ignore non-GPGGA sentences
            if (parser.offset < 5
                || parser.cmd[0] != 'G'
                || parser.cmd[1] != 'P'
                || parser.cmd[2] != 'G'
                || parser.cmd[3] != 'G'
                || parser.cmd[4] != 'A'
                || parser.cmd[5] != ',')
            {
                return false;
            }

            status = parser.getInt(5);
            fix = (status > 0);

            if (!fix)
            {
                // valid sentence, but no GPS fix
                return false;
            }

            sat = parser.getInt(6);
            lat = parser.getInt(1);
            lon = parser.getInt(3);
            return true;
        }
    }
}