using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EmailReminder.Shared.Models
{
    public class Reminder
    {
        public int Id { get; set; }
        [Required]
        [DataType(DataType.EmailAddress)]
        public string EmailAddress { get; set; }
        [Required]
        [DataType(DataType.Text)]
        public string Message { get; set; }
        [Required]
        [DataType(DataType.DateTime)]
        public DateTime DateTime { get; set; }

        public Reminder()
        {
            DateTime = DateTime.Now.AddDays(1);
        }
    }
}
