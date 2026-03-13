using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyAvaloniaApp.Models;

[Table("activites")]
public class Activite
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int Id { get; set; }

    [Column("ppr")]          public int?     Ppr         { get; set; }
    [Column("cina")]         public string?  CiNa        { get; set; }
    [Column("cd_modaffe")]   public string?  CdModaffe   { get; set; }
    [Column("cd_fonc")]      public string?  CdFonc      { get; set; }
    [Column("cd_etab")]      public string?  CdEtab      { get; set; }
    [Column("cd_service")]   public int?     CdService   { get; set; }
    [Column("cd_division")]  public int?     CdDivision  { get; set; }
    [Column("cd_entadm")]    public int?     CdEntAdm    { get; set; }
    [Column("cd_cycle")]     public string?  CdCycle     { get; set; }
    [Column("activ_princ")]  public bool?    ActivPrinc  { get; set; }

    [Column("dt_deb_exercice")] public DateTime? DtDebExercice { get; set; }
    [Column("dt_fin_exercice")] public DateTime? DtFinExercice { get; set; }
    [Column("dateaffect")]      public DateTime? DateAffect    { get; set; }
    [Column("dt_aff_etab")]     public DateTime? DtAffEtab     { get; set; }
    [Column("dt_aff_prov")]     public DateTime? DtAffProv     { get; set; }
    [Column("dt_aff_reg")]      public DateTime? DtAffReg      { get; set; }
    [Column("dt_aff_poste")]    public DateTime? DtAffPoste    { get; set; }

    // FK to Personnel
    [ForeignKey(nameof(Ppr))]
    public Personnel? Personnel { get; set; }
}