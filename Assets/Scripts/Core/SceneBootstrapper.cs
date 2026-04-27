//// Assets/Scripts/Core/SceneBootstrapper.cs
//// Managers 오브젝트에 붙임
//// 씬 안의 모든 이벤트 연결을 한 곳에서 처리 → 순환 참조 없음
//using UnityEngine;

//public class SceneBootstrapper : MonoBehaviour
//{
//    [SerializeField] private ResultUI _resultUI;
//    [SerializeField] private GameHUD _gameHUD;

//    private void Start()
//    {
//        var gm = GameManager.Instance;

//        // ResultPanel은 꺼져 있어도 Show()를 외부에서 호출
//        gm.OnGameOver += _resultUI.Show;

//        // HUD는 OnEnable에서 구독하므로 여기서는 불필요
//        // 추가 배선이 필요할 때 여기에만 추가
//    }

//    private void OnDestroy()
//    {
//        var gm = GameManager.Instance;
//        if (gm == null) return;
//        gm.OnGameOver -= _resultUI.Show;
//    }
//}