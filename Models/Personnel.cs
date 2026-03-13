using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyAvaloniaApp.Models;

[Table("personnel")]
public class Personnel
{
    [Key]
    [Column("ppr")]
    public int Ppr { get; set; }

    [Column("cina")] public string? CiNa { get; set; }
    [Column("cinn")] public int?    CiNn { get; set; }

    // Names
    [Column("nom_latin")]    public string? NomLatin    { get; set; }
    [Column("prenom_latin")] public string? PrenomLatin { get; set; }
    [Column("nom_arabe")]    public string? NomArabe    { get; set; }
    [Column("prenom_arabe")] public string? PrenomArabe { get; set; }

    // Identity
    [Column("genre")]     public string? Genre    { get; set; }
    [Column("sit_fam")]   public string? SitFam   { get; set; }
    [Column("jour_nais")] public int?    JourNais { get; set; }
    [Column("mois_nais")] public int?    MoisNais { get; set; }
    [Column("an_nais")]   public int?    AnNais   { get; set; }
    [Column("lieu_nais")] public string? LieuNais { get; set; }

    // Contact
    [Column("adresse")]      public string? Adresse     { get; set; }
    [Column("code_postal")]  public int?    CodePostal  { get; set; }
    [Column("ville")]        public string? Ville       { get; set; }
    [Column("tel_portable")] public string? TelPortable { get; set; }
    [Column("adresse_elec")] public string? AdresseElec { get; set; }

    // Professional
    [Column("cd_position")] public string? CdPosition { get; set; }
    [Column("cd_statut")]   public string? CdStatut   { get; set; }
    [Column("cd_grade")]    public string? CdGrade    { get; set; }
    [Column("cd_cadre")]    public string? CdCadre    { get; set; }
    [Column("cd_discip")]   public string? CdDiscip   { get; set; }
    [Column("cd_nation")]   public string? CdNation   { get; set; }
    [Column("cd_dips")]     public string? CdDips     { get; set; }
    [Column("cd_dipp")]     public string? CdDipp     { get; set; }
    [Column("modavgra")]    public string? Modavgra   { get; set; }
    [Column("echelon")]     public int?    Echelon    { get; set; }

    // Dates
    [Column("date_rec")]      public DateTime? DateRec      { get; set; }
    [Column("dt_titul")]      public DateTime? DtTitul      { get; set; }
    [Column("anc_adm")]       public DateTime? AncAdm       { get; set; }
    [Column("anc_grade")]     public DateTime? AncGrade     { get; set; }
    [Column("dt_echelon")]    public DateTime? DtEchelon    { get; set; }
    [Column("anc_echelon")]   public DateTime? AncEchelon   { get; set; }
    [Column("date_position")] public DateTime? DatePosition { get; set; }
    [Column("dt_grade")]      public DateTime? DtGrade      { get; set; }
    [Column("dt_cadre")]      public DateTime? DtCadre      { get; set; }
    [Column("dt_dipscol")]    public DateTime? DtDipScol    { get; set; }
    [Column("dt_dipprof")]    public DateTime? DtDipProf    { get; set; }
    [Column("dt_sitstat")]    public DateTime? DtSitStat    { get; set; }
    [Column("date_sitfam")]   public DateTime? DateSitFam   { get; set; }

    // Affiliation
    [Column("affilie")]           public bool?     Affilie          { get; set; }
    [Column("dateaffil")]         public DateTime? DateAffil        { get; set; }
    [Column("num_affil")]         public int?      NumAffil         { get; set; }
    [Column("num_imma")]          public int?      NumImma          { get; set; }
    [Column("villeform")]         public string?   VilleForm        { get; set; }
    [Column("affectation_princ")] public bool?     AffectationPrinc { get; set; }

    // Last activity snapshot
    [Column("cd_last_etab")]  public string? CdLastEtab  { get; set; }
    [Column("cd_last_fonc")]  public string? CdLastFonc  { get; set; }
    [Column("id_last_annee")] public string? IdLastAnnee { get; set; }
    [Column("id_tech")]       public int?    IdTech      { get; set; }

    // Audit
    [Column("imported_at")]
    public DateTime ImportedAt { get; set; } = DateTime.UtcNow;
}