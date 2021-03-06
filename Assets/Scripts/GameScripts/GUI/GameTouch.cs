﻿using UnityEngine;
using BE;
using UnityEngine.UI;
using Utilities;
using System;

namespace  Green
{
    public class GameTouch : MonoBehaviour, MobileRTSCamListner
    {
        public Plane xzPlane;
        public Transform CameraTransform;
        ArrowRenderer _arrowRenderer;
        void Awake()
        {
            MobileRTSCam.instance.Listner = this;
            xzPlane = new Plane(new Vector3(0f, 0f, 1f), 0f);
            _arrowRenderer = GetComponent<ArrowRenderer>();
            if(_arrowRenderer == null)
                Debug.LogError("Need Script: ArrowRender on GmaeWorld!");
            PropertyPanel = GameObject.FindObjectOfType<PlanetPropertyPanel>();
            if (PropertyPanel == null)
            {
                Debug.LogError("Need PlanetPropertyPanel in canvas!");
            }
            PropertyPanel.gameObject.SetActive(false);
            canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("Need Canvas in canvas!");
            }
            ProgressBar = GameObject.Find("ProgressBar").GetComponent<ProgressRadialBehaviour>();
            if (ProgressBar == null)
            {
                Debug.LogError("Need Progress Active in Canvas");
            }
            ProgressBar.Value = 1;
            ProgressBar.gameObject.SetActive(false);
        }

        public float ProgressDragSpeed = 1f;

        public float TotalDragLength = Screen.height;
        float _addTouchLength = 0f;
        //Vector3 _lastTouchScreenPos;
        //Vector3 _curTouchScreenPos;
        public void OnDrag(Ray ray)
        {
            if (GameManager.Instance.State != GameState.Playing) return;
                
            //_curSelectedPlanet = 
            if (DragProgressBar)
            {
                if (!_lastDragPosition.HasValue)
                {
                    _lastDragPosition = Input.mousePosition;
                    return;
                }            
                //float enter;
               // xzPlane.Raycast(ray, out enter);
                _curDragPosition = Input.mousePosition;

                //float addProgress = (_curDragPosition.Value.y - _lastDragPosition.Value.y) * ProgressDragSpeed;
                //本次增加/减少的值
                float touchLength = (_curDragPosition.Value.y - _lastDragPosition.Value.y)* ProgressDragSpeed;

                //未到整数的累加的值
                _addTouchLength += touchLength / TotalDragLength;

                //累计后增加的兵力
                var 累计后增加的兵力 = _addTouchLength * _curSelectedPlanet.PlayerSoldiers.Count;

                if (累计后增加的兵力 <= -1f)
                {
                    //取下限
                    累计后增加的兵力 = Mathf.FloorToInt(累计后增加的兵力);
                }
                else if (累计后增加的兵力 >= 1f)
                {
                    累计后增加的兵力 = Mathf.CeilToInt(累计后增加的兵力);
                }
                else
                {
                    //不超过1不显示
                    return;
                }

                //float 增加的百分比 = (int)累计后增加的兵力 / _curSelectedPlanet.PlayerSoldiers.Count;
                int 总兵力 = _curSelectedPlanet.PlayerSoldiers.Count;
                int 原来的兵力 = (int)(ProgressBar.Value / 100f * (float)总兵力);

                int 增加后的兵力 = 原来的兵力 + (int)累计后增加的兵力;

                增加后的兵力 = Mathf.Clamp(增加后的兵力, 0, 总兵力);
                //在进度条上显示
                if (_curSelectedPlanet.PlayerSoldiers.Count == 0)
                {
                    ProgressBar.Value = 100;
                }
                else
                {
                    ProgressBar.Value = (float)增加后的兵力 / (float)总兵力 * 100f;
                }

                _soldierCount.text = 增加后的兵力.ToString();

                /*
                if (touchLength > 0)
                {
                    var addProgress = touchLength / TotalDragLength;


                    OnProgressBarChanged(addProgress, ProgressBarChange.Increase);
                }
                else
                {
                    var addProgress = touchLength / TotalDragLength;
                    //var ceilProgress = Mathf.Ceil(ProgressBar.Value + addProgress);
                    OnProgressBarChanged(addProgress, ProgressBarChange.Decrease);
                }
                */
                _addTouchLength = 0;
                _lastDragPosition = _curDragPosition;
                return;
            }
            if (!_dragStart) return;
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                var planet = hit.collider.GetComponent<Planet>();
                if (planet == null) Debug.LogError("OnTouchDown() Planet == null");
                _arrowRenderer.Draw(_curSelectedPlanet.transform.position,
                    planet.transform.position);
            }
            else
            {
                Vector3 vTouch = Input.mousePosition;
                ray = Camera.main.ScreenPointToRay(vTouch);
                float enter;
                xzPlane.Raycast(ray, out enter);

                var vPickStart = ray.GetPoint(enter);
                _arrowRenderer.Draw(_curSelectedPlanet.transform.position,
                                   vPickStart);
            }
            
            
        }

        bool _dragStart = false;
        bool _destinationSelected = false;
        public void OnDragEnd(Ray ray)
        {
            if (GameManager.Instance.State != GameState.Playing) return;
            if (DragProgressBar)
            {
                DragProgressBar = false;
                _curDragPosition = null;
                _lastDragPosition = null;
                return;
            }
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                var planet = hit.collider.GetComponent<Planet>();
                if (planet == null) Debug.LogError("OnTouchDown() Planet == null");
                if (_curSelectedPlanet == null) return;
                if (planet != _curSelectedPlanet)
                {
                    _destinationPlanet = planet;
                    _arrowRenderer.Draw(_curSelectedPlanet.transform.position,
                        planet.transform.position);
                    _destinationSelected = true;
                    OnShowProgressBar();
                }
            }
            else
            {
                _destinationSelected = false;
                _curSelectedPlanet = null;
                ClearDragArrow();
                OnCloseProgressBar();
            }
            _dragStart = false;
            _curDragPosition = null;
            _lastDragPosition = null;
        }

        public ProgressRadialBehaviour ProgressBar;

        public bool DragProgressBar = false;

        void OnShowProgressBar()
        {
            DragProgressBar = true;
            ProgressBar.gameObject.SetActive(true);
            var screenPos = GameSceneCamera.WorldToScreenPoint(_destinationPlanet.transform.position);
            var tran = ProgressBar.GetComponent<RectTransform>();
            tran.transform.position = new Vector3(screenPos.x, screenPos.y, screenPos.z);    
        }

        public void OnCloseProgressBar()
        {
            ProgressBar.gameObject.SetActive(false);
            ProgressBar.Reset();
            DragProgressBar = false;
            
            ClearDragArrow();
        }

        void ClearDragArrow()
        {
            _destinationSelected = false;
            _arrowRenderer.Clear();
        }

        Vector3? _lastDragPosition;
        Vector3? _curDragPosition;
        public void OnDragStart(Ray ray)
        {
            if (DragProgressBar)
            {
                float enter;
                // xzPlane.Raycast(ray, out enter);
                //_curDragPosition = Input.mousePosition;
                //_lastDragPosition = null;
                MobileRTSCam.instance.camPanningUse = false;
                return;
            }
            RaycastHit hit;
      
            if (Physics.Raycast(ray, out hit))
            {
                MobileRTSCam.instance.camPanningUse = false;
                var planet = hit.collider.GetComponent<Planet>();
                if (planet == null) Debug.LogError("OnTouchDown() Planet == null");
                _curSelectedPlanet = planet;
                _dragStart = true;
            }
            
        }

        public void OnLongPress(Ray ray)
        {

        }

        public void OnMouseWheel(float fValue)
        {

        }

        public void OnTouch(Ray ray)
        {

        }

       
        public void OnTouchDown(Ray ray)
        {

        }

        Color _selectedColor = new Color(173f/255f, 173f/255f, 173f/255f, 1f);
        Color _defaultColor = Color.white;

        void SelectPlanet(Planet p)
        {
            var render = p.GetComponent<SpriteRenderer>();
            render.color = _selectedColor;
        }

        void CancelAllAction()
        {
            UnshowPanel();
            OnCloseProgressBar();
            _arrowRenderer.Clear();
            _destinationSelected = false;
            _curSelectedPlanet = null;
        }

        public Button _okButton;
        public Text _soldierCount;

        public Scrollbar _soldierSelectBar;

        void EnableOKButtonToSendSoldier()
        {
            _okButton.enabled = true;
        }

        //按下OK确认派兵
        public void OnClickOK()
        {
            if (_destinationSelected)
            {
                int count = int.Parse(_soldierCount.text);
                GameWorld.Instance.SendSoldier(_curSelectedPlanet, _destinationPlanet, count, SoldierType.Player);
                _destinationSelected = false;
                OnCloseProgressBar();
            }
        }

        public void OnClickSendSoldier()
        {
            _preparedToSendSoldier = true;
        }

        void UnselectPlanet()
        {
            if (_curSelectedPlanet != null)
            {
                var render = _curSelectedPlanet.GetComponent<SpriteRenderer>();
                render.color = _defaultColor;
            }
            if (_destinationPlanet != null)
            {
                var render = _destinationPlanet.GetComponent<SpriteRenderer>();
                render.color = _defaultColor;
            }
        }
        public void OnTouchUp(Ray ray)
        {
            if (DragProgressBar)
            {
                OnClickOK();
                return;
            }
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                // if raycasted object was founded, keep it to thr trPreClicked
                var planet = hit.collider.GetComponent<Planet>();
                if (planet == null) Debug.LogError("OnTouchDown() Planet == null");
                ShowPanel(planet);
            }
            else
            {
                UnshowPanel();
                _curSelectedPlanet = null;
                CancelAllAction();
            }

            _destinationSelected = false;
            ClearDragArrow();
        }

        // Use this for initialization
        void Start()
        {
            PanelOffsetY = 61.6f;
            //_soldierCount = GameObject.Find("Canvas/ProgressBar/TextPanel/Bg/Label").GetComponent<Text>();
        }

        Planet _curSelectedPlanet = null;
        Planet _destinationPlanet = null;
        enum PlanetClickState
        {
            //什么都没点
            Nothing,
           
            ShowProperty
            
        }

        bool _preparedToSendSoldier = false;

        PlanetClickState _planetClickState = PlanetClickState.Nothing;
        // Update is called once per frame
        void Update()
        {
            if (_destinationSelected)
            {
                _arrowRenderer.Draw(_curSelectedPlanet.transform.position,
                    _destinationPlanet.transform.position);
            }
            if (_destinationSelected)
            {
                var count = (int) (ProgressBar.Value * _curSelectedPlanet.PlayerSoldiers.Count / 100);
                _soldierCount.text = count.ToString();
                OnShowProgressBar();
            }
            else
            {
                _soldierCount.text = "0";
            }
        }

        enum ProgressBarChange
        {
            Increase,
            Decrease
        }

        void OnProgressBarChanged(float dt, ProgressBarChange increaseOrDecrease)
        {
            if (DragProgressBar)
            {
                float progress = (ProgressBar.Value + dt) / 100f;
                float floatSoldierToSend = progress * _curSelectedPlanet.PlayerSoldiers.Count;

                int intCount = 0;
                switch (increaseOrDecrease)
                {
                    case ProgressBarChange.Decrease:
                        intCount = Mathf.FloorToInt(floatSoldierToSend);
                        break;
                    case ProgressBarChange.Increase:
                        intCount = Mathf.CeilToInt(floatSoldierToSend);
                        break;
                }
                
                if (_curSelectedPlanet.PlayerSoldiers.Count == 0)
                {
                    ProgressBar.Value = 100;
                }
                else
                {
                    ProgressBar.Value = (((float)intCount / (float)_curSelectedPlanet.PlayerSoldiers.Count) * 100f);
                }
                _soldierCount.text = intCount.ToString();
            }    
            else
            {
                _soldierCount.text = "0";
            }
        }

        public PlanetPropertyPanel PropertyPanel;
        public Camera GameSceneCamera;
        public Canvas canvas;
        public float PanelOffsetX = 0f;
        public float PanelOffsetY = 0f;
        void ShowPanel(Planet planet)
        {
            var pos = planet.transform.position;
            var star = planet.GetComponent<Star>();
            PropertyPanel.gameObject.SetActive(true);
            var screenPos = GameSceneCamera.WorldToScreenPoint(pos);
            var tran = PropertyPanel.GetComponent<RectTransform>();
            //RectTransformUtility.WorldToScreenPoint(canvas.)
            PropertyPanel.transform.position = new Vector3(screenPos.x, screenPos.y + PanelOffsetY, screenPos.z);
            PropertyPanel.Show(star);
        }

        void UnshowPanel()
        {
            PropertyPanel.gameObject.SetActive(false);
        }

        public void OnTouchZoom(float value)
        {
            if (_curSelectedPlanet)
            {
                var screenPos = GameSceneCamera.WorldToScreenPoint(_curSelectedPlanet.transform.position);
                var ray = GameSceneCamera.ScreenPointToRay(screenPos);
                MobileRTSCam.instance.SetOrthographicTouchZoom(ray, value);
            }
        }
    }

}