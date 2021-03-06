﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Valve.VR;

namespace Immersion_VR_Agent {
    public partial class MainWindow : Window {
        private readonly BackgroundWorker worker = new BackgroundWorker {
            WorkerReportsProgress = true,
            WorkerSupportsCancellation = true
        };
        private bool isRunning = false;
        private string executablePath = "ImmersionVR.exe";
        private Agent agent = new Agent("ImmersionVR.exe");

        public MainWindow() {
            InitializeComponent();
        }

        public void UISetStatus(string icon, string title, string description) {
            statusIcon.Source = Utils.LoadBitmapFromResource(icon);
            statusLabel.Content = title;
            statusDescription.Content = description;
        }

        public void Start() {
            InvalidateVisual();

            if (agent.InitializeOpenVR()) {

                int? pid = agent.RunImmersionVR(true);

                if (pid == null) {
                    UISetStatus("openvr-error@2x.png", "Chyba!", "Nepodařilo se spustit \nImmersion VR.");
                    InvalidateVisual();
                } else {
                    StartUpdating();
                    isRunning = true;
                    runButton.IsEnabled = false;
                    runButton.Opacity = 0;
                }
            }
            else {
                UISetStatus("openvr-error@2x.png", "Chyba!", "Nepodařilo se inicializovat \nOpenVR.");
                InvalidateVisual();
            }
        }

        public void StartUpdating() {
            worker.DoWork += worker_DoWork;
            worker.ProgressChanged += worker_OnProgressChanged;
            worker.RunWorkerAsync();
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e) {
            BackgroundWorker worker = (BackgroundWorker)sender;
            while (!worker.CancellationPending) {
                if (agent.PollEvents()) {
                    worker.ReportProgress(0, "");
                }
            }
        }

        private void worker_OnProgressChanged(object sender, ProgressChangedEventArgs e) {
            switch (agent.status) {
                case AgentStatus.Ready:
                    UISetStatus("openvr-good@2x.png", "Připraveno", "");
                    break;

                case AgentStatus.AppRunning:
                    UISetStatus("openvr-good@2x.png", "Spuštěná aplikace", agent.GetRunningAppName());
                    break;

                case AgentStatus.Quitting:
                    UISetStatus("openvr@2x.png", "Ukončeno", "Znovu spusťte OpenVR.");
                    isRunning = false;
                    runButton.IsEnabled = true;
                    runButton.Opacity = 1;
                    break;
            }
        }

        private void runButton_Click(object sender, RoutedEventArgs e) {
            if (isRunning) {

            } else {
                Start();
            }
        }

        private void settingsButton_Click(object sender, RoutedEventArgs e) {
            Settings settingsWindow = new Immersion_VR_Agent.Settings();

            settingsWindow.path.Text = executablePath;
            settingsWindow.ShowDialog();
            executablePath = settingsWindow.path.Text;
            agent.ChangeExecutablePath(settingsWindow.path.Text);
        }
    }
}
