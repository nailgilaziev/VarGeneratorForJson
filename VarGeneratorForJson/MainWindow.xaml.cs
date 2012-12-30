using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace VarGeneratorForJson
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            temp.Text = @"id
Id
confirmed
num
";
        }

        private void bt_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                String varRes = "";
                String parseRes = "";
                String[] row = orig.Text.Replace("\"","").Split('\n');
                foreach (var item in row)
                {
                    String variable = item.Substring(0, item.IndexOf(":")).Trim();
                    String javaVarName = createJavaVar(variable);
                    varRes += String.Format("protected {0} {1};\n", getPossiblyVarType(variable, false), javaVarName);
                    parseRes += String.Format("{0} = j.get{1}(\"{2}\");\n", javaVarName, getPossiblyVarType(variable, true), variable);
                }
                result.Text = varRes + "\n" + parseRes;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            
        }

        private string getPossiblyVarType(string variable,bool upperFirstLetter)
        {
            String[] sep = new String[] { "\r\n" };
            String[] temps = temp.Text.Split(sep,StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in temps)
            {
                if (variable.Contains(item))
                    return upperFirstLetter ? "Int" : "int";
            }
            return "String";
        }

        private string createJavaVar(string variable)
        {
            try
            {
                for (int i = variable.IndexOf('_'); i < variable.Length && i != -1; i = variable.IndexOf('_'))
                {
                    String s1 = variable.Substring(0, i);
                    String letter = (variable[i + 1].ToString()).ToUpper();
                    String s2 = variable.Substring(i + 2);
                    variable = s1 + letter + s2;
                }
                return variable;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
            
        }
    }
}
