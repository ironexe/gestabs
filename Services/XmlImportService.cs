using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;
using MyAvaloniaApp.Data;
using MyAvaloniaApp.Models;

namespace MyAvaloniaApp.Services;

public record ImportResult(int Inserted, int Updated, int Skipped, string? ErrorMessage = null);

public class XmlImportService
{
    // ── Public entry point ────────────────────────────────────────────────────

    public async Task<ImportResult> ImportAsync(string xmlFilePath)
    {
        try
        {
            var doc  = XDocument.Load(xmlFilePath);
            var root = doc.Root ?? throw new InvalidDataException("Empty XML file.");

            // The file uses a default namespace on the root element.
            // All data child elements inherit it.
            XNamespace ns = root.Name.Namespace;

            var personnelRecords = ParsePersonnel(root, ns);
            var activiteRecords  = ParseActivites(root, ns);

            return await UpsertAsync(personnelRecords, activiteRecords);
        }
        catch (Exception ex)
        {
            return new ImportResult(0, 0, 0, ex.Message);
        }
    }

    // ── Parsing ───────────────────────────────────────────────────────────────

    private static List<Personnel> ParsePersonnel(XElement root, XNamespace ns)
    {
        var list = new List<Personnel>();

        foreach (var el in root.Elements(ns + "DATAIDENTIFPERSONNEL"))
        {
            // Skip records with no PPR (empty placeholder rows)
            var pprRaw = Val(el, ns, "PPR");
            if (string.IsNullOrWhiteSpace(pprRaw)) continue;
            if (!int.TryParse(pprRaw, out var ppr)) continue;

            list.Add(new Personnel
            {
                Ppr         = ppr,
                CiNa        = Val(el, ns, "CINA"),
                CiNn        = IntVal(el, ns, "CINN"),
                NomLatin    = Val(el, ns, "NOML"),
                PrenomLatin = Val(el, ns, "PRENOML"),
                NomArabe    = Val(el, ns, "NOMA"),
                PrenomArabe = Val(el, ns, "PRENOMA"),
                Genre       = Val(el, ns, "GENRE"),
                SitFam      = Val(el, ns, "SIT_FAM"),
                JourNais    = IntVal(el, ns, "JOUR_NAIS"),
                MoisNais    = IntVal(el, ns, "MOIS_NAIS"),
                AnNais      = IntVal(el, ns, "AN_NAIS"),
                LieuNais    = Val(el, ns, "LIEU_NAIS"),
                Adresse     = Val(el, ns, "ADRESSE"),
                CodePostal  = IntVal(el, ns, "CODE_POSTAL"),
                Ville       = Val(el, ns, "VILLE"),
                TelPortable = Val(el, ns, "TEL_PORTABLE"),
                AdresseElec = Val(el, ns, "ADRESSE_ELEC"),
                CdPosition  = Val(el, ns, "CD_POSITION"),
                CdStatut    = Val(el, ns, "CD_STATUT"),
                CdGrade     = Val(el, ns, "CD_GRADE"),
                CdCadre     = Val(el, ns, "CD_CADRE"),
                CdDiscip    = Val(el, ns, "CD_DISCIP"),
                CdNation    = Val(el, ns, "CD_NATION"),
                CdDips      = Val(el, ns, "CD_DIPS"),
                CdDipp      = Val(el, ns, "CD_DIPP"),
                Modavgra    = Val(el, ns, "MODAVGRA"),
                Echelon     = IntVal(el, ns, "ECHELON"),
                DateRec     = DateVal(el, ns, "DATE_REC"),
                DtTitul     = DateVal(el, ns, "DT_TITUL"),
                AncAdm      = DateVal(el, ns, "ANC_ADM"),
                AncGrade    = DateVal(el, ns, "ANC_GRADE"),
                DtEchelon   = DateVal(el, ns, "DT_ECHELON"),
                AncEchelon  = DateVal(el, ns, "ANC_ECHELON"),
                DatePosition = DateVal(el, ns, "DATE_POSITION"),
                DtGrade     = DateVal(el, ns, "DT_GRADE"),
                DtCadre     = DateVal(el, ns, "DT_CADRE"),
                DtDipScol   = DateVal(el, ns, "DT_DIPSCOL"),
                DtDipProf   = DateVal(el, ns, "DT_DIPPROF"),
                DtSitStat   = DateVal(el, ns, "DT_SITSTAT"),
                DateSitFam  = DateVal(el, ns, "DATE_SITFAM"),
                Affilie         = BoolVal(el, ns, "AFFILIE"),
                DateAffil       = DateVal(el, ns, "DATEAFFIL"),
                NumAffil        = IntVal(el, ns, "NumAFFIL"),
                NumImma         = IntVal(el, ns, "NumImma"),
                VilleForm       = Val(el, ns, "VILLEFORM"),
                AffectationPrinc = BoolVal(el, ns, "AffectationPrincipale"),
                CdLastEtab      = Val(el, ns, "CD_LAST_ETAB"),
                CdLastFonc      = Val(el, ns, "CD_LAST_FONC"),
                IdLastAnnee     = Val(el, ns, "ID_LAST_ANNEE"),
                IdTech          = IntVal(el, ns, "ID_tech"),
                ImportedAt      = DateTime.UtcNow,
            });
        }

        return list;
    }

    private static List<Activite> ParseActivites(XElement root, XNamespace ns)
    {
        var list = new List<Activite>();

        foreach (var el in root.Elements(ns + "ACTIVITE"))
        {
            // Need at least PPR to be useful
            var pprRaw = Val(el, ns, "PPR");
            if (string.IsNullOrWhiteSpace(pprRaw)) continue;
            if (!int.TryParse(pprRaw, out var ppr)) continue;

            list.Add(new Activite
            {
                Ppr           = ppr,
                CiNa          = Val(el, ns, "CINA"),
                CdModaffe     = Val(el, ns, "CD_MODAFFE"),
                CdFonc        = Val(el, ns, "CD_FONC"),
                CdEtab        = Val(el, ns, "CD_ETAB"),
                CdService     = IntVal(el, ns, "CD_SERVICE"),
                CdDivision    = IntVal(el, ns, "CD_DIVISION"),
                CdEntAdm      = IntVal(el, ns, "CD_ENTADM"),
                CdCycle       = Val(el, ns, "CD_CYCLE"),
                ActivPrinc    = BoolVal(el, ns, "ACTIV_PRINC"),
                DtDebExercice = DateVal(el, ns, "DT_DEB_EXERCICE"),
                DtFinExercice = DateVal(el, ns, "DT_FIN_EXERCICE"),
                DateAffect    = DateVal(el, ns, "DATEAFFECT"),
                DtAffEtab     = DateVal(el, ns, "DT_AFF_ETAB"),
                DtAffProv     = DateVal(el, ns, "DT_AFF_PROV"),
                DtAffReg      = DateVal(el, ns, "DT_AFF_REG"),
                DtAffPoste    = DateVal(el, ns, "DT_AFF_POSTE"),
            });
        }

        return list;
    }

    // ── Database upsert ───────────────────────────────────────────────────────

    private static async Task<ImportResult> UpsertAsync(
        List<Personnel> incoming,
        List<Activite>  activites)
    {
        if (incoming.Count == 0)
            return new ImportResult(0, 0, 0, "No valid employee records found in file.");

        await using var db = new AppDbContext();
        await db.Database.EnsureCreatedAsync();

        int inserted = 0, updated = 0, skipped = 0;

        var existingPprs = await db.Personnel
            .Select(p => p.Ppr)
            .ToHashSetAsync();

        foreach (var p in incoming)
        {
            if (existingPprs.Contains(p.Ppr))
            {
                // Update: overwrite all fields except ImportedAt (keep original)
                var existing = await db.Personnel.FindAsync(p.Ppr);
                if (existing is null) { skipped++; continue; }

                CopyPersonnelFields(p, existing);
                db.Personnel.Update(existing);
                updated++;
            }
            else
            {
                await db.Personnel.AddAsync(p);
                inserted++;
            }
        }

        await db.SaveChangesAsync();

        // Delete old activites for the imported PPRs, then insert fresh ones.
        // This is simpler and safer than trying to diff activity records.
        var importedPprs = incoming.Select(p => p.Ppr).ToHashSet();
        var oldActivites = db.Activites.Where(a => a.Ppr != null && importedPprs.Contains(a.Ppr.Value));
        db.Activites.RemoveRange(oldActivites);

        var validActivites = activites
            .Where(a => a.Ppr.HasValue && importedPprs.Contains(a.Ppr.Value))
            .ToList();

        await db.Activites.AddRangeAsync(validActivites);
        await db.SaveChangesAsync();

        return new ImportResult(inserted, updated, skipped);
    }

    /// <summary>Copies all mutable fields from <paramref name="src"/> into <paramref name="dst"/>.</summary>
    private static void CopyPersonnelFields(Personnel src, Personnel dst)
    {
        dst.CiNa         = src.CiNa;
        dst.CiNn         = src.CiNn;
        dst.NomLatin     = src.NomLatin;
        dst.PrenomLatin  = src.PrenomLatin;
        dst.NomArabe     = src.NomArabe;
        dst.PrenomArabe  = src.PrenomArabe;
        dst.Genre        = src.Genre;
        dst.SitFam       = src.SitFam;
        dst.JourNais     = src.JourNais;
        dst.MoisNais     = src.MoisNais;
        dst.AnNais       = src.AnNais;
        dst.LieuNais     = src.LieuNais;
        dst.Adresse      = src.Adresse;
        dst.CodePostal   = src.CodePostal;
        dst.Ville        = src.Ville;
        dst.TelPortable  = src.TelPortable;
        dst.AdresseElec  = src.AdresseElec;
        dst.CdPosition   = src.CdPosition;
        dst.CdStatut     = src.CdStatut;
        dst.CdGrade      = src.CdGrade;
        dst.CdCadre      = src.CdCadre;
        dst.CdDiscip     = src.CdDiscip;
        dst.CdNation     = src.CdNation;
        dst.CdDips       = src.CdDips;
        dst.CdDipp       = src.CdDipp;
        dst.Modavgra     = src.Modavgra;
        dst.Echelon      = src.Echelon;
        dst.DateRec      = src.DateRec;
        dst.DtTitul      = src.DtTitul;
        dst.AncAdm       = src.AncAdm;
        dst.AncGrade     = src.AncGrade;
        dst.DtEchelon    = src.DtEchelon;
        dst.AncEchelon   = src.AncEchelon;
        dst.DatePosition = src.DatePosition;
        dst.DtGrade      = src.DtGrade;
        dst.DtCadre      = src.DtCadre;
        dst.DtDipScol    = src.DtDipScol;
        dst.DtDipProf    = src.DtDipProf;
        dst.DtSitStat    = src.DtSitStat;
        dst.DateSitFam   = src.DateSitFam;
        dst.Affilie      = src.Affilie;
        dst.DateAffil    = src.DateAffil;
        dst.NumAffil     = src.NumAffil;
        dst.NumImma      = src.NumImma;
        dst.VilleForm    = src.VilleForm;
        dst.AffectationPrinc = src.AffectationPrinc;
        dst.CdLastEtab   = src.CdLastEtab;
        dst.CdLastFonc   = src.CdLastFonc;
        dst.IdLastAnnee  = src.IdLastAnnee;
        dst.IdTech       = src.IdTech;
    }

    // ── XML helper methods ────────────────────────────────────────────────────

    private static string? Val(XElement el, XNamespace ns, string name)
    {
        var v = el.Element(ns + name)?.Value;
        return string.IsNullOrWhiteSpace(v) ? null : v.Trim();
    }

    private static int? IntVal(XElement el, XNamespace ns, string name)
    {
        var v = Val(el, ns, name);
        return v is not null && int.TryParse(v, out var i) ? i : null;
    }

    private static bool? BoolVal(XElement el, XNamespace ns, string name)
    {
        var v = Val(el, ns, name);
        if (v is null) return null;
        if (bool.TryParse(v, out var b)) return b;
        return v == "1" ? true : v == "0" ? false : null;
    }

    private static DateTime? DateVal(XElement el, XNamespace ns, string name)
    {
        var v = Val(el, ns, name);
        return v is not null && DateTime.TryParse(v, out var d)
            ? DateTime.SpecifyKind(d, DateTimeKind.Utc)
            : null;
    }
}