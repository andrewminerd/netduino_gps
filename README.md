About
=====
This is a lightweight NMEA parser written in C#. It was designed for use with
a [Netduino](http://netduino.com), an "open-source electronics platform" that runs
the .NET Micro Framework. With a 48MHz ARM processor and only about 60KB of RAM,
it is extremely resource constrained. None of the existing NMEA parsers fit my
requirements for such a low powered device.

The current revision here does not parse the fields contained within the NMEA
sentences, but simply extracts each sentence and validates the checksum.

NMEA is the common name for [NMEA 0183](http://en.wikipedia.org/wiki/NMEA_0183) a
specification that defines a protocol for communication between electronic devices,
like most GPS units.

Use
=====
To use, simply create an Parser instance and feed it's `parse()` method bytes. When
the parse method returns true, a full sentence has been parsed and is available
in the `cmd` property. The length of the command is available in the `offset` property.

Currently, the Parser allocates a 1k buffer to hold the parsed commands. This is
probably far more than is actually required; if you find that you're exhausting
the RAM of your Netduino, feel free to tune this.

Example:

```c#
byte[] in;
for (int i = 0; i < in.Length; i++) {
  if (parser.parse(in[i])) {
	  System.Console.Write("Parsed a full sentence!");
	}
}
```

References
==========

To learn more about the NMEA format, see:

* http://www.gpsinformation.org/dale/nmea.htm
