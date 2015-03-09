﻿module Program

open System
open System.Drawing
open System.Windows.Forms
open System.Windows.Forms.DataVisualization.Charting
open Akka.Util.Internal
open Akka.Actor
open Akka.FSharp
open Akka.Configuration.Hocon
open System.Configuration
open Actors

let section = ConfigurationManager.GetSection "akka" :?> AkkaConfigurationSection
let chartActors = System.create "ChartActors" section.AkkaConfig

Application.EnableVisualStyles ()
Application.SetCompatibleTextRenderingDefault false

let seriesCounter = AtomicCounter(1)
let sysChart = new Chart(Name = "sysChart", Text = "sysChart", Dock = DockStyle.Fill, Location = Point(0, 0), Size = Size(684, 446), TabIndex = 0)
let form = new Form(Name = "Main", Visible = true, Text = "System Metrics", AutoScaleDimensions = SizeF(6.F, 13.F), AutoScaleMode = AutoScaleMode.Font, ClientSize = Size(684, 446))
let chartArea1 = new ChartArea(Name = "ChartArea1")
let legend1 = new Legend(Name = "Legend1")
let series1 = new Series(Name = "Series1", ChartArea = "ChartArea1", Legend = "Legend1")
let button1 = new Button(Name = "button1", Text = "Add Series", Location = Point(573, 366), Size = Size(99, 36), TabIndex = 1, UseVisualStyleBackColor = true)
sysChart.BeginInit ()
form.SuspendLayout ()
sysChart.ChartAreas.Add chartArea1
sysChart.Legends.Add legend1
sysChart.Series.Add series1
form.Controls.Add button1
form.Controls.Add sysChart
sysChart.EndInit ()
form.ResumeLayout false

let getFakeSeries counter = ChartDataHelper.randomSeries ("FakeSeries" + string (counter)) None None

let chartActor = spawn chartActors "charting" (actorOf (Actors.chartingActor sysChart))
let series = seriesCounter.GetAndIncrement () |> getFakeSeries
chartActor <! InitializeChart(Map.ofList [(series.Name, series)])

button1.Click.Add (fun _ -> 
    let series = seriesCounter.GetAndIncrement () |> getFakeSeries
    chartActor <! AddSeries(series))

[<STAThread>]    
do Application.Run (form)
