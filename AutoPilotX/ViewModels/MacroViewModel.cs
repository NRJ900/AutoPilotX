using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;
using AutoPilotX.Models;
using AutoPilotX.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;

namespace AutoPilotX.ViewModels
{
    public partial class MacroViewModel : ObservableObject
    {
        private readonly MacroService _macroService;

        [ObservableProperty]
        private string _macroName;

        [ObservableProperty]
        private ObservableCollection<MacroEvent> _macroEvents;

        public MacroViewModel()
        {
            _macroService = new MacroService();
            StartRecordingCommand = new RelayCommand(StartRecording);
            StopRecordingCommand = new RelayCommand(StopRecording);
            PlayMacroCommand = new RelayCommand(PlayMacro);
            SaveMacroCommand = new RelayCommand(SaveMacro);
            LoadMacroCommand = new RelayCommand(LoadMacro);
        }

        public ICommand StartRecordingCommand { get; }
        public ICommand StopRecordingCommand { get; }
        public ICommand PlayMacroCommand { get; }
        public ICommand SaveMacroCommand { get; }
        public ICommand LoadMacroCommand { get; }

        private void StartRecording()
        {
            _macroService.StartRecording();
        }

        private void StopRecording()
        {
            _macroService.StopRecording();
            MacroEvents = new ObservableCollection<MacroEvent>(_macroService.GetMacroEvents());
        }

        private async void PlayMacro()
        {
            await _macroService.PlayMacro(new List<MacroEvent>(MacroEvents));
        }

        private void SaveMacro()
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "JSON files (*.json)|*.json",
                DefaultExt = ".json"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                _macroService.SaveMacro(saveFileDialog.FileName, new List<MacroEvent>(MacroEvents));
            }
        }

        private void LoadMacro()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "JSON files (*.json)|*.json",
                DefaultExt = ".json"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                MacroEvents = new ObservableCollection<MacroEvent>(_macroService.LoadMacro(openFileDialog.FileName));
            }
        }
    }
}
