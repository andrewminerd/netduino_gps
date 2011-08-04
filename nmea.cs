using System;

namespace Nmea
{
  class Parser
  {
    private enum STATES : byte
    {
      begin, body, checksum1, checksum2, checksum_lr, checksum_cf, cf
    }

    private STATES state = STATES.begin;

    // probably more room than we'd need
    public byte[] cmd = new byte[1024];
    public short offset;

    private byte cmdSum;
    private byte checksum;

    /**
     * Parse a single byte. Returns true when an entire NMEA sentence has
     * been parsed; sentence is currently available in cmd (offset contains
     * length).
     */
    public bool parse(byte b)
    {
      switch (state)
      {
        case STATES.begin:
          if (b == '$')
          {
            state = STATES.body;
            offset = 0;
            cmdSum = 0;
          }
          break;
        case STATES.body:
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

    /**
     * Convert ASCII hex character to numeric value.
     */
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
  }
}

