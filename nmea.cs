using System;

namespace Nmea
{
    class Parser
    {
        private enum STATES : byte
        {
            begin, command, field_start, field_value, checksum1, checksum2, checksum_lr, checksum_cf, cf
        }

        // probably more room than we'd need
        const int BUFF_SIZE = 1024;

        private STATES state = STATES.begin;
        public byte[] cmd = new byte[BUFF_SIZE];
        public short offset;
        private byte cmdSum;
        private byte checksum;
        private byte field = 0;
        private short[] fields = new short[20];

        /// <summary>
        /// Parse a single byte. Returns true when an entire NMEA sentence has
        /// been parsed; sentence is currently available in cmd (offset contains
        /// length).
        /// </summary>
        /// <param name="b">Byte to parse</param>
        /// <returns>True when an entire sentence has been parsed</returns>
        public bool parse(byte b)
        {
            switch (state)
            {
                case STATES.begin:
                    if (b == '$')
                    {
                        state = STATES.command;
                        offset = 0;
                        cmdSum = 0;
                        field = 0;
                    }
                    break;
                case STATES.command:
                    cmd[offset++] = b;
                    cmdSum ^= b;
                    if (b == ',')
                    {
                        state = STATES.field_start;
                    }
                    break;
                case STATES.field_start:
                    if (b == '*')
                    {
                        state = STATES.checksum1;
                    }
                    else if (b == '\r')
                    {
                        // sentence terminated without a checksum
                        state = STATES.cf;
                    }
                    else
                    {
                        fields[field++] = offset;
                        cmd[offset++] = b;
                        cmdSum ^= b;
                        if (b != ',')
                        {
                            state = STATES.field_value;
                        }
                    }
                    break;
                case STATES.field_value:
                    if (b == '*')
                    {
                        state = STATES.checksum1;
                    }
                    else if (b == '\r')
                    {
                        // sentence terminated without a checksum
                        state = STATES.cf;
                    }
                    else
                    {
                        cmd[offset++] = b;
                        cmdSum ^= b;
                        if (b == ',')
                        {
                            state = STATES.field_start;
                        }
                    }
                    break;
                case STATES.checksum1:
                    checksum = (byte)(hex(b) << 4);
                    state = STATES.checksum2;
                    break;
                case STATES.checksum2:
                    checksum |= hex(b);
                    state = STATES.checksum_lr;
                    break;
                case STATES.checksum_lr:
                    // technically, this should probably choke on anything but \r
                    if (b == '\r')
                    {
                        state = STATES.checksum_cf;
                    }
                    break;
                case STATES.checksum_cf:
                    if (b == '\n')
                    {
                        state = STATES.begin;
                        if (cmdSum == checksum)
                        {
                            return true;
                        }
                    }
                    break;
                case STATES.cf:
                    // same here, could choke anything on but \n
                    if (b == '\n')
                    {
                        state = STATES.begin;
                        // indicate that a complete message was parsed
                        return true;
                    }
                    break;
            }
            return false;
        }

        /// <summary>
        /// Convert ASCII hex character to numeric value.
        /// </summary>
        /// <param name="b">Hex character in ASCII</param>
        /// <returns>Value of hex character</returns>
        private byte hex(byte b)
        {
            if (b <= '9' && b >= '0')
            {
                return (byte)(b - '0');
            }
            else if (b >= 'A' && b <= 'F')
            {
                return (byte)(b - 'A' + 10);
            }
            return 0;
        }

        public int getInt(short index)
        {
            if (index < 0 || index > field)
            {
                return -1;
            }
            int value = 0;
            for (int i = fields[index]; i < offset; i++)
            {
                if (cmd[i] == ',' || cmd[i] == '.')
                {
                    break;
                }
                value = (value * 10) + cmd[i] - '0';
            }
            return value;
        }

        /// <summary>
        /// Copies the value for the given field into the provided byte array
        /// and returns the length of the value. At most, max bytes will be
        /// copied to the array.
        /// </summary>
        /// <param name="index">Field index</param>
        /// <param name="buff">Byte array to populate with the value of the field</param>
        /// <param name="max">Maximum length of the field (generally the size of buff)</param>
        /// <returns>Length of the field put into buff, or -1 on error</returns>
        public int get(int index, byte[] buff, int max)
        {
            if (index < 0 || index > field)
            {
                return -1;
            }
            int field_len = 0;
            for (int i = fields[index]; i < offset && field_len < max; i++, field_len++)
            {
                if (cmd[i] == ',')
                {
                    break;
                }
                buff[field_len] = cmd[i];
            }
            return field_len;
        }
    }
}

