using Cognex.VisionPro;
using Cognex.VisionPro.Dimensioning;
using Cognex.VisionPro.Implementation;
using GalaSoft.MvvmLight.CommandWpf;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.Integration;

namespace Record_Example.ViewModel
{
    public class MainWindow_ViewModel
    {
        #region Fields
        private CogRecordDisplay m_Display;
        private ICogRecord m_OriginRecord;
        #endregion

        #region Commands
        //Host 로드 완료 시 호출됨
        public RelayCommand<WindowsFormsHost> HostLoaded { get; }
        public RelayCommand ImageLoadBtnClick { get; }
        public RelayCommand OverlaySaveBtnClick { get; }
        //Record 체크 시 호출됨
        public RelayCommand<string> RecordChecked { get; }
        //Record 체크 해제 시 호출됨
        public RelayCommand<string> RecordUnchecked { get; }
        #endregion

        #region Properties

        #endregion

        #region Constructor
        public MainWindow_ViewModel()
        {
            //임의의 Record 만들기 (기존 레코드가 있다면 그것을 불러와 사용)
            m_OriginRecord = new CogRecord { ContentType = typeof(ICogImage), Content = new CogImage8Grey(100, 100) };
            m_OriginRecord.SubRecords.Add(new CogRecord { RecordKey = "A Record", ContentType = typeof(CogGraphicLabel), Content = new CogGraphicLabel { Text = "A", X = 10, Y = 10, Color = CogColorConstants.Red } });
            m_OriginRecord.SubRecords.Add(new CogRecord { RecordKey = "B Record", ContentType = typeof(CogGraphicLabel), Content = new CogGraphicLabel { Text = "B", X = 50, Y = 10, Color = CogColorConstants.Orange } });
            m_OriginRecord.SubRecords.Add(new CogRecord { RecordKey = "C Record", ContentType = typeof(CogGraphicLabel), Content = new CogGraphicLabel { Text = "C", X = 10, Y = 50, Color = CogColorConstants.Yellow } });
            m_OriginRecord.SubRecords.Add(new CogRecord { RecordKey = "D Record", ContentType = typeof(CogGraphicLabel), Content = new CogGraphicLabel { Text = "D", X = 50, Y = 50, Color = CogColorConstants.Green } });

            //호출되는 내용 선언
            HostLoaded = new RelayCommand<WindowsFormsHost>(OnHostLoaded);
            RecordChecked = new RelayCommand<string>(OnChecked);
            RecordUnchecked = new RelayCommand<string>(OnUnchecked);
            ImageLoadBtnClick = new RelayCommand(OnImageLoad);
            OverlaySaveBtnClick = new RelayCommand(OnOverlaySave);
        }
        #endregion

        #region Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="host">UI에 나타낼 WindowsFormsHost</param>
        private void OnHostLoaded(WindowsFormsHost host)
        {
            //CogRecordDisplay 선언
            m_Display = new CogRecordDisplay();
            //Winform Control을 WPF UI에 연결
            host.Child = m_Display;
            //임의의 이미지 추가
            m_Display.Image = new CogImage8Grey(100, 100);
            //레코드 추가
            m_Display.Record = m_OriginRecord;
        }
        private void OnChecked(string key)
        {
            //레코드 키 값을 이용하여 레코드 존재 여부 확인 
            // -> 디스플레이엔 없고, 원본 오버레이엔 존재하는지?
            if (!m_Display.Record.SubRecords.ContainsKey(key) && m_OriginRecord.SubRecords.ContainsKey(key))
            {
                //새로운 레코드 생성
                var newRecord = new Cognex.VisionPro.Implementation.CogRecord { ContentType = typeof(ICogImage), Content = m_Display.Image };
                //기존 추가된 레코드 넣어줌
                foreach (var dispRec in m_Display.Record.SubRecords)
                {
                    newRecord.SubRecords.Add(dispRec);
                }
                //새로 추가할 레코드 넣어줌
                newRecord.SubRecords.Add(m_OriginRecord.SubRecords[key]);
                //디스플레이에 등록
                m_Display.Record = newRecord;
            }
        }
        private void OnUnchecked(string key)
        {
            //새로운 레코드 생성
            var newRecord = new Cognex.VisionPro.Implementation.CogRecord { ContentType = typeof(ICogImage), Content = m_Display.Image };
            //기존 레코드 넣어줌
            foreach (var dispRec in m_Display.Record.SubRecords)
            {
                //해당 레코드 키 값을 가진 레코드는 제외
                if (((Cognex.VisionPro.ICogRecord)dispRec).RecordKey.Equals(key)) continue;
                newRecord.SubRecords.Add(dispRec);
            }
            //디스플레이에 등록
            m_Display.Record = newRecord;
        }
        private void OnImageLoad()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Bitmap Files (*.bmp)|*.bmp"
            };
            if ((bool)dialog.ShowDialog())
            {
                m_Display.Image = new CogImage8Grey(new System.Drawing.Bitmap(dialog.FileName));
            }
        }
        private void OnOverlaySave()
        {
            var dialog = new SaveFileDialog
            {
                Filter = "Bitmap Files (*.bmp)|*.bmp"
            };
            if ((bool)dialog.ShowDialog())
            {
                //오버레이 이미지 100% 사이즈 크기를 Bitmap 파일로 변경
                var bmp = m_Display.CreateContentBitmap(Cognex.VisionPro.Display.CogDisplayContentBitmapConstants.Image).Clone() as System.Drawing.Bitmap;
                //파일 저장
                bmp.Save(dialog.FileName, System.Drawing.Imaging.ImageFormat.Bmp);
            }
        }
        #endregion
    }
}
