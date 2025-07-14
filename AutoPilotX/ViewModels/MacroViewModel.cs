using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;
using AutoPilotX.Models;
using AutoPilotX.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using WindowsInput;
using WindowsInput.Native;

namespace AutoPilotX.ViewModels
{
    public partial class MacroViewModel : ObservableObject, IDisposable
    {
        private readonly MacroService _macroService;
        private bool _disposed;

        [ObservableProperty]
        private ObservableCollection<MacroEvent> _macroEvents;

        [ObservableProperty]
        private bool _isRecording;

        [ObservableProperty]
        private bool _isPlaying;

        public MacroViewModel(MacroService macroService)
        {
            _macroService = macroService;
            StartRecordingCommand = new RelayCommand(StartRecording, () => !IsRecording && !IsPlaying);
            StopRecordingCommand = new RelayCommand(StopRecording, () => IsRecording);
            PlayMacroCommand = new RelayCommand(PlayMacro, () => !IsRecording && !IsPlaying && MacroEvents?.Count > 0);
            SaveMacroCommand = new RelayCommand(SaveMacro, () => !IsRecording && !IsPlaying && MacroEvents?.Count > 0);
            LoadMacroCommand = new RelayCommand(LoadMacro, () => !IsRecording && !IsPlaying);
            ClearMacroCommand = new RelayCommand(ClearMacro, () => !IsRecording && !IsPlaying && MacroEvents?.Count > 0);
        }

        public ICommand StartRecordingCommand { get; }
        public ICommand StopRecordingCommand { get; }
        public ICommand PlayMacroCommand { get; }
        public ICommand SaveMacroCommand { get; }
        public ICommand LoadMacroCommand { get; }
        public ICommand ClearMacroCommand { get; }

        private void StartRecording()
        {
            IsRecording = true;
            _macroService.StartRecording();
        }

        private void StopRecording()
        {
            IsRecording = false;
            _macroService.StopRecording();
            MacroEvents = new ObservableCollection<MacroEvent>(_macroService.GetMacroEvents());
        }

        private async void PlayMacro()
        {
            IsPlaying = true;
            await _macroService.PlayMacro(new List<MacroEvent>(MacroEvents));
            IsPlaying = false;
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

        private void ClearMacro()
        {
            MacroEvents.Clear();
        }

        public void Dispose()
        {
            if (_disposed) return;
            _macroService.Dispose();
            _disposed = true;
        }

        ~MacroViewModel() => Dispose();
    }
}
