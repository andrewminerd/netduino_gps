About
=====
This is a lightweight NMEA parser written in C#. It was designed for use with
a [Netduino](http://netduino.com), an "open-source electronics platform" that runs
the .NET Micro Framework. With a 48MHz ARM processor and only about 60KB of RAM,
it is extremely resource constrained. None of the existing NMEA parsers fit my
requirements for such a low powered device.

NMEA is the common name for [NMEA 0183](http://en.wikipedia.org/wiki/NMEA_0183): a
specification that defines a protocol for communication between electronic devices,
like most GPS units. By default, NMEA compatible GPS devices communicate over a serial
port at 4800 baud, 8N1.

Use
=====
To use, simply create a Parser instance and feed bytes to its `parse()` method. When
the parse method returns true, a full sentence has been parsed and is available
in the `cmd` property. The length of the command is available in the `offset` property.

Currently, the Parser allocates a 1k buffer to hold the parsed commands. This is
probably far more than is actually required; if you find that you're running out
of memory, feel free to tune this.

Example:

```c#
byte[] in;
for (int i = 0; i < in.Length; i++) {
  if (parser.parse(in[i])) {
    System.Console.Write("Parsed a full sentence!");
  }
}
```

The Parser object itself does not fetch data, so you'll be responsible for getting data from whatever source you'd like to use. For example, to provide the Parser data from a serial port:

```c#
namespace gps_test
{
  public class Program
  {
    static NMEA.Parser parser = new NMEA.Parser();

    private static void onRecv(Object sender, SerialDataReceivedEventArgs args) {
      SerialPort input = (SerialPort)sender;
      int avail = input.BytesToRead;
      while (avail-- > 0) {
        if (parser.parse(input.ReadByte())) {
          Debug.Print(new String(Encoding.UTF8.GetChars(parser.cmd)));
        }
      }
    }

    public static void Main() {
      SerialPort input = new SerialPort(SerialPorts.COM1, 4800, 0, 8, StopBits.One);
      input.DataReceived += onRecv;
      input.Open();
      while (true);
    }
  }
}
```

While parsing each sentence, the parser records the starting offset of each field. You can retrieve the value of specific fields using the get* methods on the parser object.

For examples of parsing specific NMEA sentences, see the `examples` directory.

References
==========

To learn more about the NMEA format, see:

* http://www.gpsinformation.org/dale/nmea.htm
* http://en.wikipedia.org/wiki/NMEA_0183
