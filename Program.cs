using System;
using System.Text;
using Microsoft.Data.Sqlite;


namespace ChangeDbForWordOfDay
{
    class Program
    {
        public enum OldColumn: int
        {
            _id = 0,
            word,
            explanation,
            example,
            basym,
            condition,
            symbol,
            favorite,
            date
        }

        private const string WORD_TABLE = "word";
        private const string EXPLANATION_TABLE = "explanationBash";

        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            using var connection = new SqliteConnection("Data Source=wordOfDay.db");
            string createWordsTable = $"\"{WORD_TABLE}\" ( `_id` INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE, `word` TEXT NOT NULL UNIQUE, `symbol` TEXT NOT NULL, `favorite` INTEGER, `date` TEXT )";
            CreateTable(createWordsTable, connection);
            string createExplanationTable = $"\"{EXPLANATION_TABLE}\" ( `_id` INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE, `wordId` INTEGER NOT NULL, `word` TEXT NOT NULL, `explanation` TEXT NOT NULL, `example1` TEXT,`example2` TEXT,`example3` TEXT, `basym` TEXT, `condition` TEXT )";
            CreateTable(createExplanationTable, connection);
            DbConnection(connection);
        }

        static void CreateTable(string text, SqliteConnection connection)
        {
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = $"CREATE TABLE IF NOT EXISTS {text}";
            var dd = command.ExecuteNonQuery();
            connection.Close();
        }   
        static void InsertWordsTable(SqliteConnection connection, string id, string word, string symbol)
        {
            var command = connection.CreateCommand();
            command.CommandText = $"INSERT INTO {WORD_TABLE}(_id, word, symbol) VALUES({id}, '{word}', '{symbol}')";
            command.ExecuteNonQuery();
        }
        static string SetTextValue(string value)
        {
            return value != null ? $"'{value}'" : "NULL";
        }
        static void InsertExplanationTable(SqliteConnection connection, int id, string wordId, string word, string explanation, string example1, string example2, string example3, string basym, string condition)
        {
            var command = connection.CreateCommand();
            command.CommandText = $"INSERT INTO {EXPLANATION_TABLE}(_id, wordId, word, explanation, example1, example2, example3, basym, condition) VALUES({id}, {wordId}, '{word}', '{explanation}', {SetTextValue(example1)}, {SetTextValue(example2)}, {SetTextValue(example3)}, {SetTextValue(basym)}, {SetTextValue(condition)})";
            try
            {

                command.ExecuteNonQuery();
            }
            catch
            {
                Console.WriteLine($"Exception: {word} {explanation} {command.CommandText}");
            }
        }
        static void DbConnection(SqliteConnection connection)
        {
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText =
            @"
                    SELECT *
                    FROM words
            ";
            using var reader = command.ExecuteReader();
            int explanationId = 1;
            while (reader.Read())
            {
                string id = reader.GetString(OldColumn._id);
                string word = reader.GetString(OldColumn.word);
                string symbol = reader.GetString(OldColumn.symbol);

                string explanationCommon = reader.GetString(OldColumn.explanation);
                string exampleCommon = reader.GetString(OldColumn.example);
                string basymCommon = reader.GetString(OldColumn.basym);
                string conditionCommon = reader.GetString(OldColumn.condition);

                string[] explanations = explanationCommon.Replace('|', '/').Split("/");
                string[] examples = exampleCommon?.Split("#");
                string[] basyms = basymCommon?.Split("#");
                string[] conditions = conditionCommon?.Split("#");

                for (int i = 0; i < explanations.Length; i++)
                {
                    string basym = null, condition = null;
                    string[] example = new string[3];
                    if (examples != null && i < examples.Length)
                    {
                        string[] exTmp = examples[i].Split("|");
                        for (int j = 0; j < exTmp.Length && j < 3; j++)
                        {
                            
                            example[j] = exTmp[j];
                        }
                    }
                    if (basyms != null && i < basyms.Length)
                    {
                        basym = basyms[i];
                    }
                    if (conditions != null && i < conditions.Length)
                    {
                        condition = conditions[i];
                    }
                    InsertExplanationTable(connection, explanationId++, id, word, explanations[i], example[0], example[1], example[2], basym, condition);
                }


                InsertWordsTable(connection, id, word, symbol);
                Console.WriteLine($"success: {id}");
            }
            connection.Close();
            Console.WriteLine("Finish!");
            Console.ReadLine();
        }
    }
    public static class ExtensionSqlite
    {
        public static string GetString(this SqliteDataReader reader, object col) 
        {
            if (reader.IsDBNull((int)col)) return null;
            return reader.GetString((int)col);
        }
    }
}
