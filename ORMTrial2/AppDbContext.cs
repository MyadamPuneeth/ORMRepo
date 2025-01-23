//using ORMTrial2.Models;
using ORMTrial2.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORMTrial2
{
    public class AppDbContext : DbFrame
    {
        private readonly CRUDOperationsManger crudOpsManager;

        public AppDbContext()
        {
            crudOpsManager = new CRUDOperationsManger();
        }

        //public DbFrame<User> User { get; set; }
        //public DbFrame<Student> Student { get; set; }

        public void CRUDopsMethod()
        {

            //User userInsertData = new User
            //{
            //    Age = 3,
            //    UserName="Tapasya"
            //};

            //crudOpsManager.InsertData(userInsertData);

            //User updateData = new User
            //{
            //    UserName = "Hero Chan"
            //};

            //crudOpsManager.UpdateData(updateData, "age = 2");

            //Student stuInsertData = new Student
            //{
            //    stuname = "Vijay",
            //    rollNUmber = 1,
            //};
            //crudOpsManager.InsertData(stuInsertData);



        }

    }

}
