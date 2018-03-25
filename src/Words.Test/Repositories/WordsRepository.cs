﻿using System;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using Dapper;
using Words.Test.Repositories.Models;

namespace Words.Test.Repositories
{
    public interface IWordsRepository
    {
        WordPair[] GetAllWords();
        WordPair[] GetUnknownWords();
        bool CheckIfUnknownWordAlreadyExist(int wordId);
        bool AddNewUnknownWord(int wordId);
        bool RemoveLearnedUnknownWord(int wordId);
    }

    public class WordsRepository : IWordsRepository
    {
        private readonly string _dbFolder;
        private readonly string _dbFile;
        private readonly string _connectionString;

        public WordsRepository()
        {
            _dbFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\SilverWords";
            _dbFile = $"{_dbFolder}\\Vocabulary.db";
            _connectionString = $"Data Source={_dbFile};Version=3;UseUTF16Encoding=True;Password=FJtkLXz2aBeBARdW;";
        }

        public WordPair[] GetAllWords()
        {
            using (var dbConnection = new SQLiteConnection(_connectionString))
            {
                dbConnection.Open();

                const string getAllWordsQuery =
                    @"SELECT 
                        AW.Id, AW.FirstLanguageWord, AW.SecondLanguageWord, 
                        AW.LanguagePair, AW.CreatedAt, AW.ModifiedAt
                      FROM AllWords AW";

                WordPair[] allWords = dbConnection.Query<WordPair>(getAllWordsQuery).ToArray();

                return allWords;
            }
        }

        public WordPair[] GetUnknownWords()
        {
            using (var dbConnection = new SQLiteConnection(_connectionString))
            {
                dbConnection.Open();

                const string getUnknownWordsQuery =
                    @"SELECT 
                        AW.Id, AW.FirstLanguageWord, AW.SecondLanguageWord, 
                        AW.LanguagePair, AW.CreatedAt, AW.ModifiedAt  
                      FROM UnknownWords UW
                      JOIN AllWords AW ON UW.ID_AllWords = AW.Id";

                WordPair[] unknownWords = dbConnection.Query<WordPair>(getUnknownWordsQuery).ToArray();

                return unknownWords;
            }
        }

        public bool CheckIfUnknownWordAlreadyExist(int wordId)
        {
            using (var dbConnection = new SQLiteConnection(_connectionString))
            {
                dbConnection.Open();

                const string checkIfUnknownWordAlreadyExistsQuery =
                    @"SELECT EXISTS(
                        SELECT 1 
                        FROM UnknownWords
                        WHERE ID_AllWords = @WordId
                        LIMIT 1
                      )";
                var queryParameters = new {WordId = wordId};

                bool exist = dbConnection.QuerySingle<bool>(checkIfUnknownWordAlreadyExistsQuery, queryParameters);

                return exist;
            }
        }

        public bool AddNewUnknownWord(int wordId)
        {
            using (var dbConnection = new SQLiteConnection(_connectionString))
            {
                dbConnection.Open();

                const string insertNewUnknownWordCommand =
                    @"INSERT INTO UnknownWords
                      VALUES(NULL, @WordId)";
                var queryParameters = new {WordId = wordId};

                int affectedRows = dbConnection.Execute(insertNewUnknownWordCommand, queryParameters);

                return affectedRows == 1;
            }
        }

        public bool RemoveLearnedUnknownWord(int wordId)
        {
            using (var dbConnection = new SQLiteConnection(_connectionString))
            {
                dbConnection.Open();

                const string deleteLearnedUnknownWordCommand =
                    @"DELETE FROM UnknownWords
                      WHERE ID_AllWords = @WordId";
                var queryParameters = new {WordId = wordId};

                int affectedRows = dbConnection.Execute(deleteLearnedUnknownWordCommand, queryParameters);

                return affectedRows == 1;
            }
        }

        public void InitializeDatabaseIfNotExist()
        {
            if (File.Exists(_dbFile)) return;

            Directory.CreateDirectory(_dbFolder);
            SQLiteConnection.CreateFile(_dbFile);

            using (var dbConnection = new SQLiteConnection(_connectionString))
            {
                dbConnection.Open();

                CreateAllWordsTable(dbConnection);

                CreateUnknownWordsTable(dbConnection);

                CreateTestResultsTable(dbConnection);

                FillAllWordsTable(dbConnection);
            }
        }

        private void CreateAllWordsTable(SQLiteConnection dbConnection)
        {
            const string createAllWordsTableSql =
                @"CREATE TABLE [AllWords] (
                    [Id] INTEGER NOT NULL
                    , [FirstLanguageWord] TEXT NOT NULL
                    , [SecondLanguageWord] TEXT NOT NULL
                    , [LanguagePair] INTEGER NOT NULL
                    , [CreatedAt] TEXT NOT NULL
                    , [ModifiedAt] TEXT NOT NULL
                    , CONSTRAINT [PK_AllWords] PRIMARY KEY ([Id])
                    , UNIQUE(FirstLanguageWord, SecondLanguageWord, LanguagePair)
                );";

            SQLiteCommand createAllWordsTableCommand = new SQLiteCommand(createAllWordsTableSql, dbConnection);
            createAllWordsTableCommand.ExecuteNonQuery();
        }

        private void CreateUnknownWordsTable(SQLiteConnection dbConnection)
        {
            const string createUnknownWordsTableSql =
                @"CREATE TABLE [UnknownWords] (
                      [Id] INTEGER NOT NULL
                    , [Id_AllWords] INTEGER NOT NULL
                    , CONSTRAINT [PK_UnknownWords] PRIMARY KEY ([Id])
                    , FOREIGN KEY([Id_AllWords]) REFERENCES AllWords([Id])
                );";

            SQLiteCommand createUnknownWordsTableCommand = new SQLiteCommand(createUnknownWordsTableSql, dbConnection);
            createUnknownWordsTableCommand.ExecuteNonQuery();
        }

        private void CreateTestResultsTable(SQLiteConnection dbConnection)
        {
            const string createTestResultsTableSql =
                @"CREATE TABLE [TestResults] (
                      [Id] INTEGER NOT NULL
                    , [FinishedAt] TEXT NOT NULL
                    , [DurationAsTicks] TEXT NOT NULL
                    , [WordsType] INTEGER NOT NULL
                    , [LanguagePair] INTEGER NOT NULL
                    , [SelectedLanguage] INTEGER NOT NULL
                    , [TestType] INTEGER NOT NULL
                    , [NumberOfTotalWords] INTEGER NOT NULL
                    , [LearnedWordsAsJson] TEXT NOT NULL
                    , [KnownWordsAsJson] TEXT NOT NULL
                    , [NewUnknownWordsAsJson] TEXT NOT NULL
                    , [UnknownWordsAsJson] TEXT NOT NULL
                    , CONSTRAINT [PK_TestResults] PRIMARY KEY ([Id])
                );";

            SQLiteCommand createTestResultsTableCommand = new SQLiteCommand(createTestResultsTableSql, dbConnection);
            createTestResultsTableCommand.ExecuteNonQuery();
        }

        private void FillAllWordsTable(SQLiteConnection dbConnection)
        {
            const string fillAllWordsTableCommand =
                @"BEGIN TRANSACTION;
	        INSERT INTO 'AllWords' VALUES (NULL, 'Pirmadienis', 'Monday', 1, '2018-03-24 10:09:03', '2018-03-24 10:09:03');
	        INSERT INTO 'AllWords' VALUES (NULL, 'Šiandien', 'Today', 1, '2018-03-24 10:09:03', '2018-03-24 10:09:03');
	        INSERT INTO 'AllWords' VALUES (NULL, 'Metai', 'Year', 1, '2018-03-24 10:09:03', '2018-03-24 10:09:03');
	        INSERT INTO 'AllWords' VALUES (NULL, 'Laikrodis', 'Clock', 1, '2018-03-24 10:09:03', '2018-03-24 10:09:03');
	        INSERT INTO 'AllWords' VALUES (NULL, 'Valanda', 'Hour', 1, '2018-03-24 10:09:03', '2018-03-24 10:09:03');
	        INSERT INTO 'AllWords' VALUES (NULL, 'Pavasaris', 'Spring', 1, '2018-03-24 10:09:03', '2018-03-24 10:09:03');
	        INSERT INTO 'AllWords' VALUES (NULL, 'Viso gero', 'Good bye', 1, '2018-03-24 10:09:03', '2018-03-24 10:09:03');
	        INSERT INTO 'AllWords' VALUES (NULL, 'Senelis', 'Grandfather, Grandpa, Grandad', 1, '2018-03-24 10:09:03', '2018-03-24 10:09:03');
	        INSERT INTO 'AllWords' VALUES (NULL, 'Grybas', 'Mushroom', 1, '2018-03-24 10:09:03', '2018-03-24 10:09:03');
	        INSERT INTO 'AllWords' VALUES (NULL, 'Kuprinė', 'Book bag', 1, '2018-03-24 10:09:03', '2018-03-24 10:09:03');
	        INSERT INTO 'AllWords' VALUES (NULL, 'Žalia', 'Green', 1, '2018-03-24 10:09:03', '2018-03-24 10:09:03');
	        INSERT INTO 'AllWords' VALUES (NULL, 'Dramblys', 'Elephant', 1, '2018-03-24 10:09:03', '2018-03-24 10:09:03');
	        INSERT INTO 'AllWords' VALUES (NULL, 'Laiptai', 'Stairs', 1, '2018-03-24 10:09:03', '2018-03-24 10:09:03');
	        INSERT INTO 'AllWords' VALUES (NULL, 'Sapnas, Svajonė', 'Dream', 1, '2018-03-24 10:09:03', '2018-03-24 10:09:03');
	        INSERT INTO 'AllWords' VALUES (NULL, 'Apie ką tu kalbi?', 'What you talking about?', 1, '2018-03-24 10:09:03', '2018-03-24 10:09:03');
	        INSERT INTO 'AllWords' VALUES (NULL, 'Man patinka mėlynas dangus', 'I like blue sky', 1, '2018-03-24 10:09:03', '2018-03-24 10:09:03');
                COMMIT;";

            SQLiteCommand createTestResultsTableCommand = new SQLiteCommand(fillAllWordsTableCommand, dbConnection);
            createTestResultsTableCommand.ExecuteNonQuery();
        }
    }
}
