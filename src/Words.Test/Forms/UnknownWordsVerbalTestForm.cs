﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using Words.Test.Constants;
using Words.Test.Enums;
using Words.Test.Models;
using Words.Test.Services;

namespace Words.Test.Forms
{
    public partial class UnknownWordsVerbalTestForm : Form
    {
        private readonly IWordsService _wordsService;
        private readonly SelectedLanguage _selectedLanguage;

        private int _currentUnknownWordPairId;

        private WordPair[] _unknownWords;
        private readonly IList<WordPair> _learnedWords = new List<WordPair>();
        private int _startingCountOfUnknownWords;

        private readonly Stopwatch _stopWatch = new Stopwatch();

        public UnknownWordsVerbalTestForm(SelectedLanguage selectedLanguage)
        {
            SetTestTimer();

            _wordsService = new WordsService();

            _selectedLanguage = selectedLanguage;

            InitializeComponent();

            SetSelectedLanguage(_selectedLanguage);
        }

        private void SetTestTimer()
        {
            var timer = new Timer();
            timer.Tick += timer_Tick;
            timer.Interval = 1000;
            timer.Enabled = true;
            _stopWatch.Start();
            timer.Start();
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            TestTimerLabel.Text = _stopWatch.Elapsed.Hours.ToString("00") + @":" +
                                  _stopWatch.Elapsed.Minutes.ToString("00") + @":" +
                                  _stopWatch.Elapsed.Seconds.ToString("00");
        }

        private void UnknownWordsTestForm_Load(object sender, EventArgs e)
        {
            NextWordButton.Select();

            _unknownWords = _wordsService.GetRandomlySortedUnknownWords();
            _startingCountOfUnknownWords = _unknownWords.Length;

            _currentUnknownWordPairId = _unknownWords[0].Id;

            ProgressLabel.Text = _unknownWords.Length.ToString();
            LearnedWordsCountLinkLabel.Enabled = false;

            WordPair firstUnknownWord = _unknownWords.First();
            LtWordTextBox.Text = firstUnknownWord.FirstLanguageWord;
            EnWordTextBox.Text = firstUnknownWord.SecondLanguageWord;
        }

        private void UnknownWordsVerbalTestForm_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == KeyCodes.Enter)
            {
                if (NextWordButton.Visible)
                {
                    HandleNextWordButtonClickedEvent();
                }

                e.Handled = true;
            }
            else if (e.KeyChar == KeyCodes.Backspace)
            {
                if (IDontKnowTheWordButton.Visible)
                {
                    HandleIDontKnowWordButtonClickedEvent();
                }
            }
        }

        private void NextWordButton_Click(object sender, EventArgs e)
        {
            HandleNextWordButtonClickedEvent();
        }

        private void HandleNextWordButtonClickedEvent()
        {
            if (IDontKnowTheWordButton.Visible)
            {
                _learnedWords.Add(_unknownWords.First(uw => uw.Id == _currentUnknownWordPairId));
                LearnedWordsCountLinkLabel.Text = _learnedWords.Count.ToString();

                _unknownWords = _unknownWords.Where(unknownWord => unknownWord.Id != _currentUnknownWordPairId).ToArray();
            }
            else
            {
                SetSelectedLanguage(_selectedLanguage);
            }

            LearnedWordsCountLinkLabel.Enabled = _learnedWords.Count > 0;
            IDontKnowTheWordButton.Visible = true;

            if (_unknownWords.Length > 0)
            {
                ProgressLabel.Text = _unknownWords.Length.ToString();

                WordPair nextUnknownWord = _unknownWords.First();

                LtWordTextBox.Text = nextUnknownWord.FirstLanguageWord;
                EnWordTextBox.Text = nextUnknownWord.SecondLanguageWord;
                _currentUnknownWordPairId = nextUnknownWord.Id;
            }
            else
            {
                HandleFinishedTest();
            }

            switch (_selectedLanguage)
            {
                case SelectedLanguage.Lithuanian:
                    LtWordTextBox.Visible = false;
                    break;
                case SelectedLanguage.English:
                    EnWordTextBox.Visible = false;
                    break;
                case SelectedLanguage.Mixed:
                    bool currentEnLabelVisibleState = EnWordTextBox.Visible;
                    bool currentLtLabelVisibleState = LtWordTextBox.Visible;
                    EnWordTextBox.Visible = !currentEnLabelVisibleState;
                    LtWordTextBox.Visible = !currentLtLabelVisibleState;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void IDontKnowTheWordButton_Click(object sender, EventArgs e)
        {
            HandleIDontKnowWordButtonClickedEvent();
        }

        private void HandleIDontKnowWordButtonClickedEvent()
        {
            IDontKnowTheWordButton.Visible = false;
            NextWordButton.Focus();

            EnWordTextBox.Visible = true;
            LtWordTextBox.Visible = true;

            var unknownWordToMove = _unknownWords[0];

            _unknownWords = _unknownWords.Where(unknownWord => unknownWord.Id != _currentUnknownWordPairId).ToArray();

            var unknownWordsWithoutUnknownWord = _unknownWords.ToList();
            unknownWordsWithoutUnknownWord.Add(unknownWordToMove);

            _unknownWords = unknownWordsWithoutUnknownWord.ToArray();
        }

        private void EndTestButton_Click(object sender, EventArgs e)
        {
            HandleFinishedTest();
        }

        private void SetSelectedLanguage(SelectedLanguage selectedLanguage)
        {
            if (selectedLanguage == SelectedLanguage.Lithuanian)
            {
                LtWordTextBox.Visible = false;
                EnWordTextBox.Visible = true;
            }
            else if (selectedLanguage == SelectedLanguage.English)
            {
                LtWordTextBox.Visible = true;
                EnWordTextBox.Visible = false;
            }
            else
            {
                LtWordTextBox.Visible = false;
                EnWordTextBox.Visible = true;
            }
        }

        private void LearnedWordsCountLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            List<string> learnedWordsToDisplay =
                _learnedWords.Select(learnedWord => learnedWord.FirstLanguageWord + " - " + learnedWord.SecondLanguageWord).ToList();

            string showWordsFormName = "Išmokti žodžiai:";
            var showWordsListByTypeForm = new ShowWordsListByTypeForm(showWordsFormName, learnedWordsToDisplay);

            showWordsListByTypeForm.Activate();
            showWordsListByTypeForm.ShowDialog(this);
        }

        private void HandleFinishedTest()
        {
            _stopWatch.Stop();

            this.Hide();
            
            List<string> learnedWordsToDisplay =
                _learnedWords.Select(learnedWord => learnedWord.FirstLanguageWord + " - " + learnedWord.SecondLanguageWord).ToList();
            var testResultsForm = new TestResultsForm(_selectedLanguage, TestType.Verbal, WordsType.UnknownWords, _stopWatch, _startingCountOfUnknownWords, learnedWordsToDisplay);
            testResultsForm.Closed += (s, args) => this.Close();

            testResultsForm.Show();
        }
    }
}
