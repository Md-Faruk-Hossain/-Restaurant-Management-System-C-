
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Restaurant_Management_System.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Restaurant_Management_System
{
    /// <summary>
    /// Interaction logic for Update.xaml
    /// </summary>
    public partial class Update : Window
    {
        public MainWindow mainWindow => (MainWindow)Owner;  //Access MainWindow by Owner

        public string filename = @"employee.json";
        public FileInfo TempImageFile { get; set; }         //Using for upload image
        public FileInfo OldImageFile { get; set; }          //Using for exists image
        public Update()
        {
            
            InitializeComponent();
            List<string> Designation = new List<string>()
            {
                  "Manager",
                  "Cashier",
                  "Waiter",
                  "Chef"
            };
            this.CmbDesignation.ItemsSource = Designation;
            CmbDesignation.Text = "Manager.";

            List<string> Gender = new List<string>()
            {
                  "Male",
                  "Female",

            };
            this.CmbGender.ItemsSource = Gender;
            CmbGender.Text = "Male.";
           
        }

        private void BtnUpdate_Click(object sender, RoutedEventArgs e)
        {
            var EmployeeId = Convert.ToInt32(TextEmpId.Text);            
            var Name = TextName.Text;
            var Designation = CmbDesignation.SelectedItem.ToString();
            var PhoneNo = TextPhoneNo.Text;
            var Age = TextAge.Text;
            var NationalID = TextNationalId.Text;
            var Email = TextEmail.Text;
            var Address = TextAddress.Text;
            var Gender = CmbGender.Text;
            var City = TextCity.Text;
            var Country = TextCountry.Text;
            var Date = DatePicker.SelectedDateProperty;



            var json = File.ReadAllText(filename);
            var jsonObj = JObject.Parse(json);
            var empJson = jsonObj.GetValue("Employee").ToString();
            var empList = JsonConvert.DeserializeObject<List<Employee>>(empJson);

            foreach (var item in empList.Where(x => x.EmployeeId == EmployeeId))
            {
               
                item.Name = Name;
                item.Designation = Designation;
                item.PhoneNo = PhoneNo;
                item.Age = Age;
                item.NationalId = NationalID;
                item.Email = Email;
                item.Address = Address;
                item.Gender = Gender;
                item.City = City;
                item.Country = Country;
                item.Date = DateJoin.DisplayDate;
               
                OldImageFile = (item.ImageTitle != "default.png") ? new FileInfo(mainWindow.GetImagePath() + item.ImageTitle) : null;   //ternary to evaluate null if exists image is default image

                if (TempImageFile != null && OldImageFile == null)  //Check if upload image not null && exists image is null or default.png
                {
                    TempImageFile.CopyTo(mainWindow.GetImagePath() + item.EmployeeId + TempImageFile.Extension);
                    item.ImageTitle = item.EmployeeId + TempImageFile.Extension;
                    TempImageFile = null;
                }
                if (OldImageFile != null && TempImageFile != null && File.Exists(OldImageFile.FullName)) //Check if upload image not null && old image not null. Extra -> check if old file exists in directory
                {
                    item.ImageTitle = item.EmployeeId + TempImageFile.Extension;
                    OldImageFile.Delete();      //Delete exists image
                    TempImageFile.CopyTo(mainWindow.GetImagePath() + EmployeeId + TempImageFile.Extension); //Copy upload image to target directory
                    TempImageFile = null;
                }

            }

            var empArray = JArray.FromObject(empList);  //Convert List<Emoloyee> to Jarray
            jsonObj["Employee"] = empArray;            //Set Jarray to 'Employees' JProperty
            string output = JsonConvert.SerializeObject(jsonObj, Formatting.Indented);  //Serialize data using Extension Method
            File.WriteAllText(filename, output);

            this.Close();                               //Close the current window

            //mainWindow.Showdata();                 
            MainWindow main = new MainWindow();
            main.Show();                                         //Call Mainwindow ShowData() Method
            MessageBox.Show("Data Updated Successfully !!");
            
            this.Close();
            
        }

        private void BtnImgModify_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fd = new OpenFileDialog();
            fd.Filter = "Image Files(*.jpg; *.jpeg; *.png;)|*.jpg; *.jpeg; *.png;";
            fd.Title = "Select an Image";
            if (fd.ShowDialog().Value == true)
            {
                ImageModify.Source = mainWindow.ImageInstance(new Uri(fd.FileName));  //ImageInstance return new instance of image rather than image reference
                TempImageFile = new FileInfo(fd.FileName);

                //MainWindow m = new MainWindow();
                //this.Hide();
                //m.Show();
                //Back
            }
        }

        
    }
}
