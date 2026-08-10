using AQMS.Worker;

namespace AQMS.Tests;

// ParseTemperature ist bewusst static und IO-frei -> ohne Sensor, ohne Pi, ohne
// Dateisystem testbar. Genau dafür wurde das Parsen vom Lesen getrennt.
public class Ds18b20ReaderTests
{
    [Fact]
    public void ParseTemperature_GueltigeRohdaten_LiefertCelsius()
    {
        // Originalformat des Kernel-Treibers: Zeile 1 CRC, Zeile 2 Rohwert in Milligrad
        var rohdaten = "8f 01 55 05 7f a5 a5 66 1a : crc=1a YES\n"
                     + "8f 01 55 05 7f a5 a5 66 1a t=24937\n";

        var ergebnis = Ds18b20Reader.ParseTemperature(rohdaten);

        // double-Vergleich mit Toleranz - exakte Gleichheit ist bei Fließkomma riskant
        Assert.NotNull(ergebnis);
        Assert.Equal(24.937, ergebnis!.Value, precision: 3);
    }

    [Fact]
    public void ParseTemperature_CrcFehlgeschlagen_LiefertNull()
    {
        // CRC "NO" = Übertragungsfehler. Lieber KEIN Wert als ein falscher Wert.
        var rohdaten = "8f 01 55 05 7f a5 a5 66 1a : crc=1a NO\n"
                     + "8f 01 55 05 7f a5 a5 66 1a t=24937\n";

        Assert.Null(Ds18b20Reader.ParseTemperature(rohdaten));
    }

    [Fact]
    public void ParseTemperature_OhneTMarker_LiefertNull()
    {
        var rohdaten = "8f 01 55 05 7f a5 a5 66 1a : crc=1a YES\n"
                     + "8f 01 55 05 7f a5 a5 66 1a\n";

        Assert.Null(Ds18b20Reader.ParseTemperature(rohdaten));
    }

    [Fact]
    public void ParseTemperature_NurEineZeile_LiefertNull()
    {
        Assert.Null(Ds18b20Reader.ParseTemperature("8f 01 : crc=1a YES\n"));
    }

    [Fact]
    public void ParseTemperature_LeererInhalt_LiefertNull()
    {
        // Kann bei einem abgebrochenen sysfs-Read real vorkommen
        Assert.Null(Ds18b20Reader.ParseTemperature(""));
    }

    [Theory]
    [InlineData("t=abc")]      // kein int
    [InlineData("t=")]         // nichts hinter dem Marker
    public void ParseTemperature_UnparsbarerWert_LiefertNull(string zweiteZeile)
    {
        var rohdaten = "8f 01 55 05 7f a5 a5 66 1a : crc=1a YES\n" + zweiteZeile + "\n";

        Assert.Null(Ds18b20Reader.ParseTemperature(rohdaten));
    }

    [Fact]
    public void ParseTemperature_NegativeTemperatur_LiefertNegativenWert()
    {
        // Der Sensor kann bis -55 °C. int.TryParse muss das Minus mitnehmen.
        var rohdaten = "8f 01 55 05 7f a5 a5 66 1a : crc=1a YES\n"
                     + "8f 01 55 05 7f a5 a5 66 1a t=-3250\n";

        var ergebnis = Ds18b20Reader.ParseTemperature(rohdaten);

        Assert.NotNull(ergebnis);
        Assert.Equal(-3.25, ergebnis!.Value, precision: 3);
    }
}