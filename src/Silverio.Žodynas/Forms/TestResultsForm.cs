﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using Silverio.Žodynas.Enums;

namespace Silverio.Žodynas.Forms
{
    public partial class TestResultsForm : Form
    {
        private readonly SelectedLanguage _selectedLanguage;
        private readonly TestType _testType;
        private readonly WordsType _wordsType;
        private readonly Stopwatch _elapsedTimeStopWatch;
        private readonly List<string> _lernedWordsForStats;
        private readonly List<string> _unknownWordsForStats;

        public TestResultsForm(SelectedLanguage selectedLanguage, TestType testType, WordsType wordsType, Stopwatch elapsedTimeStopWatch, List<string> lernedWordsForStats = null, List<string> unknownWordsForStats = null)
        {
            _selectedLanguage = selectedLanguage;
            _testType = testType;
            _wordsType = wordsType;
            _elapsedTimeStopWatch = elapsedTimeStopWatch;
            _lernedWordsForStats = lernedWordsForStats;
            _unknownWordsForStats = unknownWordsForStats;

            InitializeComponent();
        }

        private void TestResultsForm_Load(object sender, EventArgs e)
        {
            switch (_selectedLanguage)
            {
                case SelectedLanguage.Lithuanian:
                    SelectedLanguageLabel.Text = @"Lietuvių";
                    break;
                case SelectedLanguage.English:
                    SelectedLanguageLabel.Text = @"Anglų";
                    break;
                case SelectedLanguage.Mixed:
                    SelectedLanguageLabel.Text = @"Maišyta";
                    break;
                default:
                    SelectedLanguageLabel.Text = @"Nenustatyta";
                    break;
            }

            switch (_testType)
            {
                case TestType.Grammar:
                    SelectedTestTypeLabel.Text = @"Raštu";
                    break;
                case TestType.Verbal:
                    SelectedTestTypeLabel.Text = @"Žodžiu";
                    break;
                default:
                    SelectedTestTypeLabel.Text = @"Nenustatyta";
                    break;
            }

            switch (_wordsType)
            {
                case WordsType.AllWords:
                    WordsTypeLabel.Text = @"Visi";
                    break;
                case WordsType.UnknownWords:
                    WordsTypeLabel.Text = @"Nežinomi";
                    break;
                default:
                    WordsTypeLabel.Text = @"Nenustatyta";
                    break;
            }

            TimeSpan timeSpan = _elapsedTimeStopWatch.Elapsed;
            ElapsedTimeLabel.Text = $@"{timeSpan.Hours}h {timeSpan.Minutes}m {timeSpan.Seconds}s";

            if (_lernedWordsForStats != null)
            {
                LearnedWordsStatsHeaderLabel.Visible = true;
                LearnedWordsStatsLabel.Visible = true;
                LearnedWordsStatsLabel.Text = $@"{_lernedWordsForStats.Count}";
            }
            
            if (_unknownWordsForStats != null)
            {
                UnknownWordsStatsHeaderLabel.Visible = true;
                UnknownWordsStatsLabel.Visible = true;
                UnknownWordsStatsLabel.Text = $@"{_unknownWordsForStats.Count}";
            }
        }

        private void LearnedWordsStatsLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string showWordsFormName = "Žinomi žodžiai:";
            var showWordsListByTypeForm = new ShowWordsListByTypeForm(showWordsFormName, _lernedWordsForStats);

            showWordsListByTypeForm.Activate();
            showWordsListByTypeForm.ShowDialog(this);
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string showWordsFormName = "Nežinomi žodžiai:";
            var showWordsListByTypeForm = new ShowWordsListByTypeForm(showWordsFormName, _unknownWordsForStats);

            showWordsListByTypeForm.Activate();
            showWordsListByTypeForm.ShowDialog(this);
        }

        private void StartDifferentTestButton_Click(object sender, EventArgs e)
        {
            this.Hide();
            
            var startupForm = new StartupForm();
            startupForm.Closed += (s, args) => this.Close();

            startupForm.Show();
        }

        private void EndProgramButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
