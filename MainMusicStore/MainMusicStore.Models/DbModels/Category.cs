using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainMusicStore.Models.DbModels
{
  public  class Category
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage ="Kategori Adı boş geçilemez")]
        [StringLength(250,MinimumLength =3,ErrorMessage ="Kategori Adı uzunluğu 3 ile 250 karakter olmalıdır.")]
        public string CategoryName { get; set; }

    }
}
