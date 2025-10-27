namespace TiendaVirtual.Model;

public class TarjetaFicticia
{
    public int Id { get; set; }
    public int Numero { get; set; }

    public string TarjPassword { get; set; }
    public decimal Saldo { get; set; } = GenerarSaldoFicticio();

    public static decimal GenerarSaldoFicticio()
    {
        Random rnd = new();
        return rnd.Next(200, 10001); // Genera entre 200 y 10,000
    }
}
