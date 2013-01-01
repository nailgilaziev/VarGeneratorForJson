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
            tb_exclusionVars.Text = @"id
Id
confirmed
num
";
        }

        private void bt_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                String tableName=tb_jtableName.Text;
                List<string> vars=getJSONObjectVars(tb_JSONObjectStruct.Text.Replace("\"", ""));
                List<string> javaVars = getJavaVars(vars);

                List<string> javaVarsDeclaration = getJavaVarsDeclaration(javaVars,vars);
                List<string> javaJsonParse = getJsonParseCode(javaVars, vars);
                List<string> tvars = getTableVarsDeclaration(vars);                
                
                tb_jvars.Text = listToString(javaVarsDeclaration);
                tb_jparse.Text = listToString(javaJsonParse);
                tb_tvars.Text = listToString(tvars);

                if (tableName != "")
                {
                    tb_tcolumns.Text = getTableColumnsCreateCode(tableName, vars);
                    tb_tmodel_create.Text = getCreateModelCode(tableName, vars);
                    tb_tmodel_insert.Text = getInsertModelCode(tableName, vars, javaVars);

                    tb_table_code.Text = getTableCode(tableName, vars, javaVars);
                }
                if (tabControl.SelectedIndex == 0)
                    tabControl.SelectedIndex = 1;



            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

        }
        /**
         * Возвращает список переменных JSONObject'a котторый был передан для parsing'a
         */
        private List<string> getJSONObjectVars(String varDescriptionRows)
        {
            List<string> vars = new List<string>();
            String[] rows = varDescriptionRows.Replace("\"", "").Split('\n');
            foreach (var row in rows)
            {
                String variable = row.Substring(0, row.IndexOf(":")).Trim();
                vars.Add(variable);
            }
            return vars;
        }

        /**
         * Возвращает список имен Java переменных 
         */
        private List<string> getJavaVars(List<string> vars)
        {
            List<string> javaVars = new List<string>();
            foreach (var variable in vars)
            {
                String javaVar = createJavaVar(variable);
                javaVars.Add(javaVar);
            }
            return javaVars;
        }

        /**
         * Возвращает описание Java переменных 
         */
        private List<string> getJavaVarsDeclaration(List<string> javaVars, List<string> vars)
        {
            List<string> javaVarsDeclaration = new List<string>();
            for (int i = 0; i < javaVars.Count; i++)
            {
                String t = String.Format("protected {0} {1};", getPossiblyVarType(vars[i], false), javaVars[i]);
                javaVarsDeclaration.Add(t);
            }
            return javaVarsDeclaration;
        }

        /**
         * Возвращает код парсинга JSONObject'a в java переменные
         */
        private List<string> getJsonParseCode(List<string> javaVars, List<string> vars)
        {
            List<string> parseList = new List<string>();
            for (int i = 0; i < javaVars.Count; i++)
            {
                String t = String.Format("{0} = j.get{1}(\"{2}\");", javaVars[i], getPossiblyVarType(vars[i], true), vars[i]);
                parseList.Add(t);
            }
            return parseList;
        }


        private string getPossiblyVarType(string variable, bool upperFirstLetter)
        {
            String[] sep = new String[] { "\r\n" };
            String[] temps = tb_exclusionVars.Text.Split(sep, StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in temps)
            {
                if (variable.Contains(item))
                    return upperFirstLetter ? "Int" : "int";
            }
            return "String";
        }

        private string createJavaVar(string variable)
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

        /// <summary>
        /// описания переменных  в классе Table
        /// </summary>
        /// <param name="vars"></param> имена переменных json объекта
        /// <returns></returns>
        private List<string> getTableVarsDeclaration(List<string> vars)
        {
            List<string> tVars = new List<string>();
            foreach (var variable in vars)
            {
                var varDeclaration = String.Format("public static final String {0}=\"{1}\";",variable.ToUpper(),variable);
                tVars.Add(varDeclaration);
            }
            return tVars;
        }

        /// <summary>
        ////Код создания колонок
        /// </summary>
        /// <param name="vars">имена переменных json объекта</param>
        /// <returns></returns>
        private string getTableColumnsCreateCode(String tableName, List<string> vars)
        {
            String code = String.Format(@"
public T{0}(Database database) {{
super(database);
table_name=""{0}"";
addColumns(
", tableName);
            for (int i = 0; i < vars.Count; i++)
            {
                String lineTrail = i < vars.Count - 1 ? "," : "";
                if (getPossiblyVarType(vars[i], true) == "String")
                    code += String.Format("new Column({0}){1}\n", vars[i].ToUpper(), lineTrail);
                else
                    code += String.Format("new Column({0},Types.INTEGER){1}\n", vars[i].ToUpper(), lineTrail);
            }
            code += @");
}";
            return code;
        }

        private string getCreateModelCode(String tableName, List<string> vars)
        {
            String code = String.Format(@"

private J{0} get{0}(Cursor c){{
return new J{0}(
", tableName.Substring(0,tableName.Length-1));
            for (int i = 0; i < vars.Count; i++)
            {
                String lineTrail = i < vars.Count - 1 ? "," : "";
                if (getPossiblyVarType(vars[i], true) == "String")
                    code += String.Format("c.getString(c.getColumnIndex({0})){1}\n", vars[i].ToUpper(), lineTrail);
                else
                    code += String.Format("c.getInt(c.getColumnIndex({0})){1}\n", vars[i].ToUpper(), lineTrail);
            }
            code += @");
}";
            return code;
        }

        private string getInsertModelCode(String tableName, List<string> vars, List<string> javaVars)
        {
            String tableNameLower = tableName.ToLower();
            tableNameLower=tableNameLower.Substring(0, tableNameLower.Length - 1);
            String code = String.Format(@"

public Long insert(J{0} {1}){{
ContentValues cv=new ContentValues();
", tableName.Substring(0, tableName.Length - 1), tableNameLower);
            for (int i = 0; i < vars.Count; i++)
            {
                String javaVarUpFirstLetter = javaVars[i];
                javaVarUpFirstLetter = javaVarUpFirstLetter[0].ToString().ToUpper() + javaVarUpFirstLetter.Substring(1);
                code += String.Format("cv.put({0},{1}.get{2}());\n", vars[i].ToUpper(), tableNameLower, javaVarUpFirstLetter);
            }
            code += @"
Long id=insert(cv);						
return id;
}";
            return code;
        }

        private string getOtherCode(String tableName)
        {
            return String.Format(@"

    public List<J{0}> get(String  whereClause) {{
		List<J{0}> t=new ArrayList<J{0}>();
		Cursor cursor=select(whereClause);						
		while(cursor.moveToNext())
			t.add(get{0}(cursor));			
		cursor.close();			
		db.close();
		return t;
	}}

	public List<J{0}> getAll() {{
		return get(null);
	}}

	public void deleteAll() {{
		delete(null);
	}}
", tableName.Substring(0, tableName.Length - 1));
        }

        string getTableCode(String tableName, List<string> vars, List<string> javaVars)
        {
            String code = String.Format(@"public class T{0} extends Table {{

", tableName);
            code += listToString(getTableVarsDeclaration(vars))
            + getTableColumnsCreateCode(tableName, vars)
            + getCreateModelCode(tableName, vars)
            + getInsertModelCode(tableName, vars, javaVars)
            + getOtherCode(tableName)
            + "}";
            return code;
        }

        string listToString(List<string> list)
        {
            string s = "";
            foreach (var item in list)
                s += item + "\n";
            return s;
        }
    }
}
