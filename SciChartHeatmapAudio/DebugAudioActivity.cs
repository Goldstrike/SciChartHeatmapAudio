﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using SciChart.Charting.Visuals;
using SciChart.Charting.Visuals.Axes;
using SciChart.Data.Model;
using SciChart.Charting.Model.DataSeries;
using SciChart.Charting.Visuals.RenderableSeries;
using SciChart.Drawing.Common;
using Android.Graphics;
using SciChart.Charting.Visuals.PointMarkers;
using SciChart.Charting.Modifiers;
using System.Timers;
using SciChartHeatmapAudio.CustomViews;
using System.Threading;
using SciChartHeatmapAudio.Helpers;
using System.Threading.Tasks;
using SciChartHeatmapAudio.Services;
using static SciChartHeatmapAudio.Helpers.WvlLogger;
using SciChart.Core.Model;

namespace SciChartHeatmapAudio
{

    [Activity(Label = "DebugAudioActivity")]
    public class DebugAudioActivity : Activity
    {
        // SciChart Surfaces
        public SciChartSurface Surface => FindViewById<SciChartSurface>(Resource.Id.firstChart);
        public SciChartSurface HeatmapSurface => FindViewById<SciChartSurface>(Resource.Id.secondChart);

        // DataSeries
        XyDataSeries<int, int> samplesDataSeries;
        XyDataSeries<int, int> fftDataSeries;

        // UniformHeatmapDataSeries
        //UniformHeatmapDataSeries<int, int, int> heatmapSeries = new UniformHeatmapDataSeries<int, int, int>(width, height);
        UniformHeatmapDataSeries<int, int, int> heatmapSeries = new UniformHeatmapDataSeries<int, int, int>(new int[width, height], 0, 10000, 0, 1);


        public static int width = 1024;
        public static int height = 1024;
        //public static int width = 128;
        //public static int height = 1024;

        public int[] Data = new int[width * height];

        // Others
        int samplesCount = 2048;
        CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
        CancellationToken token;
        int lastElement = 0;

        int samplesUpdatedCount = 0;
        int xChart1count = 0;
        int xChart2count = 0;

        // AudioService
        AudioService audioService;



        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_debugaudio);

            // Initialize charts
            InitCharts();


            // Start AudioService
            token = cancelTokenSource.Token;
            Task.Run(() =>
            {
                //var audioService = new AudioService();
                audioService = new AudioService();
                audioService.samplesUpdated += AudioService_samplesUpdated;
                audioService.StartRecord();

            }, token);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            audioService.samplesUpdated -= AudioService_samplesUpdated;
            audioService.StopRecord();
        }

        private void InitCharts()
        {
            InitFirstChart();
            InitSecondChart();
        }


        private void InitFirstChart()
        {
            WvlLogger.Log(LogType.TraceAll,"InitFirstChart()");

            var xAxis = new NumericAxis(this)
            {
                AutoRange = AutoRange.Always,
                DrawMajorBands = false,
                DrawLabels = false,
                DrawMajorTicks = false,
                DrawMinorTicks = false,
                DrawMajorGridLines = false,
                DrawMinorGridLines = false,
                AxisTitle = "Time (seconds)"
            };

            var yAxis = new NumericAxis(this)
            {
                AutoRange = AutoRange.Never,
                VisibleRange = new DoubleRange(short.MinValue, short.MaxValue),
                DrawMajorBands = false,
                DrawLabels = false,
                DrawMajorTicks = false,
                DrawMinorTicks = false,
                DrawMajorGridLines = false,
                DrawMinorGridLines = false,
                AxisTitle = "Freq (hZ)"
            };

            samplesDataSeries = new XyDataSeries<int, int> { FifoCapacity = new Java.Lang.Integer(500000) };
            var rs = new FastLineRenderableSeries { DataSeries = samplesDataSeries };

            using (Surface.SuspendUpdates())
            {
                Surface.XAxes.Add(xAxis);
                Surface.YAxes.Add(yAxis);
                Surface.RenderableSeries.Add(rs);
            }
        }

        private void InitSecondChart()
        {
            WvlLogger.Log(LogType.TraceAll,"InitFirstChart()");

            var xAxis = new NumericAxis(this)
            {
                AutoRange = AutoRange.Always,
                //AutoRange = AutoRange.Never,
                DrawMajorBands = false,
                DrawLabels = false,
                DrawMajorTicks = false,
                DrawMinorTicks = false,
                DrawMajorGridLines = false,
                DrawMinorGridLines = false,
                FlipCoordinates = true,
                AxisAlignment = AxisAlignment.Left,
                //VisibleRange = new DoubleRange(0, 10)
                //VisibleRange = new DoubleRange(0, 5)
                //VisibleRange = new DoubleRange(0, 128)
            };

            var yAxis = new NumericAxis(this)
            {
                AutoRange = AutoRange.Always,
                //AutoRange = AutoRange.Never,
                DrawMajorBands = false,
                DrawLabels = false,
                DrawMajorTicks = false,
                DrawMinorTicks = false,
                DrawMajorGridLines = false,
                DrawMinorGridLines = false,
                FlipCoordinates = true,
                AxisAlignment = AxisAlignment.Bottom,
                //VisibleRange = new DoubleRange(0, 10000)
                //VisibleRange = new DoubleRange(-3000, 20000)
                //VisibleRange = new DoubleRange(-300,2048)
            };

            // from XF sample
            /*
            var rs = new FastUniformHeatmapRenderableSeries
            {
                DataSeries = heatmapSeries,
                ColorMap = new SciChart.Charting.Visuals.RenderableSeries.ColorMap(
                    new int[] { Color.Transparent, Color.DarkBlue, Color.Purple, Color.Red, Color.Yellow, Color.White },
                    new float[] { 0f, 0.2f, 0.4f, 0.6f, 0.8f, 1f }
                    )
            };
            */       

            // from Android.Kotlin sample
            var rs = new FastUniformHeatmapRenderableSeries
            {
                DataSeries = heatmapSeries,
                Maximum = 70,
                Minimum = -30,
                //Maximum = 280,
                //Minimum = -120,
                ColorMap = new SciChart.Charting.Visuals.RenderableSeries.ColorMap(
                    new int[] { Color.Transparent, Color.DarkBlue, Color.Purple, Color.Red, Color.Yellow, Color.White },
                    new float[] { 0f, 0.0001f, 0.25f, 0.50f, 0.75f, 1f }
                //ColorMap = new SciChart.Charting.Visuals.RenderableSeries.ColorMap(
                //    new int[] { Color.Transparent, Color.Yellow, Color.Orange, Color.Red, Color.Purple, Color.Black },
                //    new float[] { 0f, 0.05f, 0.10f, 0.15f, 0.20f, 1f }
                )
            };
            

            #region Zoom and Pan

            // Create interactivity modifiers
            var pinchZoomModifier = new PinchZoomModifier();
            pinchZoomModifier.SetReceiveHandledEvents(true);

            var zoomPanModifier = new ZoomPanModifier();
            zoomPanModifier.SetReceiveHandledEvents(true);

            var zoomExtentsModifier = new ZoomExtentsModifier();
            zoomExtentsModifier.SetReceiveHandledEvents(true);

            var yAxisDragModifier = new YAxisDragModifier();
            yAxisDragModifier.SetReceiveHandledEvents(true);

            // Create modifier group from declared modifiers
            var modifiers = new ModifierGroup(pinchZoomModifier, zoomPanModifier, zoomExtentsModifier, yAxisDragModifier);

            #endregion

            using (HeatmapSurface.SuspendUpdates())
            {
                HeatmapSurface.XAxes.Add(xAxis);
                HeatmapSurface.YAxes.Add(yAxis);
                HeatmapSurface.RenderableSeries.Add(rs);
                HeatmapSurface.ChartModifiers.Add(modifiers);
            }

        }

        private void AudioService_samplesUpdated(object sender, System.EventArgs e)
        {
            WvlLogger.Log(LogType.TraceAll,"AudioService_samplesUpdated()");
            samplesUpdatedCount++;

            var audioService = (AudioService)sender;

            if (token.IsCancellationRequested)
            {
                WvlLogger.Log(LogType.TraceAll,"AudioService_samplesUpdated() - token.IsCancellationRequested");
                audioService.StopRecord();
                return;
            }

            var arguments = e as SamplesUpdatedEventArgs;

            if (arguments != null)
            {
                WvlLogger.Log(LogType.TraceAll,"AudioService_samplesUpdated() - arguments != null");
                var samples = arguments.UpdatedSamples;
                if (samples.Length < samplesCount)
                {
                    WvlLogger.Log(LogType.TraceValues,"AudioService_samplesUpdated() - samples.Length < samplesCount - sample.Length : " + samples.Length.ToString() + " samplesCount : " + samplesCount.ToString());
                    return;
                }
                WvlLogger.Log(LogType.TraceValues, "AudioService_samplesUpdated() - sample.Length : " + samples.Length.ToString());

                
                //samplesDataSeries.YValues = samples;
                UpdateSamplesDataSeries(samples);
                var fftValues = audioService.FFT(samples);

                //fftDataSeries.YValues = fftValues;
                //UpdateFFTDataSeries(fftValues);

                //heatmapSeries.AppenData(fftValues);
                UpdateHeatmapDataSeries(fftValues);
                

                /*
                List<int> tempSamples = new List<int>();
                int cpt = 0;
                foreach (var sam in samples)
                {
                    cpt++;
                    if (cpt % 2 == 0)
                        tempSamples.Add(sam);
                }
                int[] halfSamples = tempSamples.ToArray();

                UpdateSamplesDataSeries(halfSamples);
                var fftValues = audioService.FFT(halfSamples);

                //fftDataSeries.YValues = fftValues;
                //UpdateFFTDataSeries(fftValues);

                //heatmapSeries.AppenData(fftValues);
                UpdateHeatmapDataSeries(fftValues);
                */
            }
        }

        #region Update series (SampleData, FFTData, HeatMap)

        private void UpdateSamplesDataSeries(int[] dataSeries)
        {
            WvlLogger.Log(LogType.TraceAll,"UpdateSamplesDataSeries()");


            xChart1count = samplesUpdatedCount * dataSeries.Length;

            var xArray = new int[dataSeries.Length];
            for (int i = 0; i < dataSeries.Length; i++)
            {
                xArray[i] = lastElement++;
            }
            try
            {
                samplesDataSeries.Append(xArray, dataSeries);
                WvlLogger.Log(LogType.TraceValues,"UpdateSamplesDataSeries() - xarray : " + xArray.Length.ToString() + " - dataSeries : " + dataSeries.Sum().ToString());
            }
            catch (Exception e)
            {
                WvlLogger.Log(LogType.TraceExceptions,"UpdateSamplesDataSeries() - exception : " + e.ToString());
            }
        }

        private void UpdateFFTDataSeries(int[] fftValues)
        {
            WvlLogger.Log(LogType.TraceAll,"UpdateFFTDataSeries()");

            var xArray = new int[fftValues.Length];
            for (int i = 0; i < fftValues.Length; i++)
            {
                xArray[i] = i;
            }

            try
            {
                if (fftDataSeries.Count == 0)
                {
                    fftDataSeries.Append(xArray, fftValues);
                    WvlLogger.Log(LogType.TraceValues,"UpdateSamplesDataSeries() - xarray : " + xArray.Length.ToString() + " - fftValues : " + fftValues.Sum().ToString());
                }
                else
                {
                    fftDataSeries.UpdateRangeYAt(0, fftValues);
                    WvlLogger.Log(LogType.TraceValues,"UpdateSamplesDataSeries() - 0 - fftValues : " + fftValues.Sum().ToString());
                }
            }
            catch (Exception e)
            {
                WvlLogger.Log(LogType.TraceExceptions,"UpdateFFTDataSeries() - exception : " + e.ToString());
            }
        }

        //public void AppenData(int[] data)
        public void UpdateHeatmapDataSeries(int[] data)
        {
            WvlLogger.Log(LogType.TraceAll,"UpdateHeatmapDataSeries()");
            //WvlLogger.Log(LogType.TraceAll,"UpdateHeatmapDataSeries - Width : " + width.ToString() + " - Height : " + height.ToString());
            WvlLogger.Log(LogType.TraceValues,"UpdateHeatmapDataSeries() - Data before Array.Copy() : " + Data.Sum().ToString());

            var spectrogramSize = width * height;
            var fftSize = data.Length;
            var offset = spectrogramSize - fftSize;

            xChart2count = samplesUpdatedCount * data.Length;

            WvlLogger.Log(LogType.TraceValues,"UpdateHeatmapDataSeries() - set offset : " + offset.ToString());

            try
            {
                Array.Copy(Data, fftSize, Data, 0, offset);
                Array.Copy(data, 0, Data, offset, fftSize);
                WvlLogger.Log(LogType.TraceValues,"UpdateHeatmapDataSeries() - Data after Array.Copy() : " + Data.Sum().ToString());

                heatmapSeries.UpdateZValues(Data);
                WvlLogger.Log(LogType.TraceAll,"UpdateSamplesDataSeries() - UpdateZValues()");
            }
            catch (Exception e)
            {
                WvlLogger.Log(LogType.TraceExceptions,"UpdateSamplesDataSeries() - exception : " + e.ToString());
            }
        }

        #endregion

    }
}