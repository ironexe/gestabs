using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyAvaloniaApp.Models;

[Table("app_settings")]
public class AppSettings
{
    [Key]
    [Column("id")]
    public int Id { get; set; } = 1;

    [Column("institution_name")]
    public string InstitutionName { get; set; } = "ثانوية الإمام الجزولي الاعدادية";

    [Column("academy_name")]
    public string AcademyName { get; set; } = "سوس ماسة";

    [Column("region_name")]
    public string RegionName { get; set; } = "انزكان ايت ملول";

    [Column("academic_year")]
    public string AcademicYear { get; set; } = "2025/2026";

    [Column("director_name")]
    public string DirectorName { get; set; } = "محمد ايت دحمانت";
}