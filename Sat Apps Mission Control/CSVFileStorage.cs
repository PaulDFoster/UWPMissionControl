using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace Sat_Apps_Mission_Control
{
    public sealed partial class MainPage : Page
    {
        Windows.Storage.StorageFile CSVFile;

        public async void CreateNewCSVFile()
        {
            Windows.Storage.StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            string fileName = string.Format("SatKit_{0}.csv", DateTime.Now.ToString("ddMMyyyy HHmmss"));
            CSVFile = await storageFolder.CreateFileAsync(fileName, Windows.Storage.CreationCollisionOption.ReplaceExisting);
            await WriteSIKDataHeader();
        }

        public void CloseCurrentCSVFile()
        {
            
        }

        public async Task WriteSIKData(string data)
        {
            // write SIK data out as CSV
            for (int x = 0; x < data.Length; x++)
            {
                switch (data[x])
                {
                    case '{':
                    case '}':
                    case ':':
                    case 'N':
                    case 'I':
                    case 'U':
                    case 'V':
                    case 'T':
                    case 'X':
                    case 'Y':
                    case 'Z':
                    case 'H':
                    case 'P':
                    case 'R':
                        break;
                    default:
                        await Windows.Storage.FileIO.WriteTextAsync(CSVFile, data[x].ToString());
                        break;

                }
                
            }
            await Windows.Storage.FileIO.WriteTextAsync(CSVFile, "\n");
        }

        public async Task WriteSIKDataHeader()
        {
            await Windows.Storage.FileIO.WriteTextAsync(CSVFile, "Id,IR,UV,Visible,Temp,X,Y,Z,Heading,Pitch, Roll\n");

        }
    }
}
