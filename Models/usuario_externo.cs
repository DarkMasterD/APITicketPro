using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class usuario_externo
{
    [Key]
    public int id_usuario_externo { get; set; }

    public int id_usuario { get; set; }

    [ForeignKey("id_usuario")]
    public usuario usuario { get; set; }

    public string nombre { get; set; }
    public string apellido { get; set; }
    public string empresa { get; set; }
}
