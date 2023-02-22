using Shop.Models.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Shop.Models.ViewModels.Account
{
    public class UserVM
    {
        public UserVM() { }
        public UserVM(UserDTO row)
        {
            Id = row.Id;
            FirstName = row.FirstName;
            LastName = row.LastName;
            EmailAdress = row.EmailAdress;
            UserName = row.UserName;
            Password = row.Password;
        }
        public int Id { get; set; }
        [Required(ErrorMessage = "Поле имя не может быть пустым")]
        public string FirstName { get; set; }
        [Required(ErrorMessage = "Поле фамилия не может быть пустым")]
        public string LastName { get; set; }
        [Required(ErrorMessage = "Поле электронный адрес не может быть пустым")]
        [DataType(DataType.EmailAddress)]
        public string EmailAdress { get; set; }
        [Required(ErrorMessage = "Поле логин не может быть пустым")]
        public string UserName { get; set; }
        [Required(ErrorMessage = "Поле пароль не может быть пустым")]
        public string Password { get; set; }
        [Required(ErrorMessage = "Повторите пароль")]
        public string ConfirmPassword { get; set; }
    }
}