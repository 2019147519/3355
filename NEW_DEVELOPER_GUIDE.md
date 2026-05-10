# 3355 Unity 프로젝트 신입 개발자 가이드

이 문서는 처음 프로젝트에 들어온 개발자가 구조를 빠르게 파악하고, 어디를 수정해야 하는지 감을 잡기 위한 온보딩 문서입니다. 기준 분석일은 2026-05-10이며, 프로젝트 루트는 `C:\Users\songs\Desktop\Lion\UnityProjects\3355`입니다.

## 1. 프로젝트 한눈에 보기

`3355`는 15x15 오목/렌주 기반 Unity 게임 프로젝트입니다. 현재 구현의 중심은 한 화면에서 두 명이 번갈아 두는 싱글 플레이와 AI 대전입니다. 멀티플레이, 카드 시스템, 백엔드 로그인은 코드 뼈대가 있으나 아직 완성 기능으로 보기 어렵습니다.

주요 특징은 다음과 같습니다.

- Unity 버전: `6000.3.13f1`
- 렌더 파이프라인: URP
- 보드 크기: 15x15
- 플레이어 값: `None = 0`, `Black = 1`, `White = 2`
- 흑돌 금수 규칙: 3-3, 4-4, 장목 검사
- AI: 후보 수 생성 + 휴리스틱 평가 + Alpha-Beta 탐색 + Zobrist 해시 캐시
- UI: 메인 메뉴, 게임 HUD, 일시정지, 결과/재대결, 설정, 토스트
- 입력: 마우스 클릭과 모바일 터치를 모두 처리

## 2. 실행과 기본 확인

Unity Hub에서 프로젝트를 열 때는 Unity `6000.3.13f1` 버전을 맞추는 것이 가장 안전합니다.

빌드 세팅에 등록된 씬은 다음 두 개입니다.

- `Assets/Scenes/LoginScene.unity`
- `Assets/Scenes/GameScene.unity`

실제 게임 플레이 흐름은 `GameScene` 쪽 핵심 매니저와 UI에 집중되어 있습니다. 에디터에서 바로 테스트할 때는 `GameScene`을 열고 Play 하는 방식으로 먼저 확인하는 것을 권장합니다.

패키지는 `Packages/manifest.json`에서 관리됩니다. 주요 패키지는 `Universal RP`, `UGUI`, `TextMeshPro`, `Input System`, `Unity Test Framework`, `AI Navigation`, `The Backend SDK` 계열입니다.

## 3. 폴더 구조

```text
Assets/
  Scenes/                 실제 게임 씬
  Scripts/
    Core/                 게임 흐름, 보드, 턴, 승패, 입력, 씬/오디오
    Modes/                Single, AI, Multi 게임 모드 전략
    AI/                   오목 AI와 보드 평가
    Rendering/            3D 보드/돌/카메라/이펙트 렌더링
    UI/                   메뉴, HUD, 결과, 설정, 토스트 UI
    Cards/                카드 시스템 뼈대
    Backend/              The Backend SDK 테스트성 코드
  Prefabs/                흑돌/백돌 프리팹
  Effects/                승리 이펙트 프리팹
  Settings/               URP/렌더링 설정
ProjectSettings/          Unity 프로젝트 설정
Packages/                 Unity 패키지 매니페스트
```

`Assets/TextMesh Pro/Examples & Extras` 아래 파일은 TextMeshPro 샘플입니다. 게임 로직 분석 시에는 대부분 무시해도 됩니다.

## 4. 핵심 런타임 흐름

게임의 가장 중요한 진입점은 `GameManager`입니다.

```text
MainMenuUI 버튼 클릭
  -> GameManager.StartGame(mode)
  -> BoardManager.Init()
  -> TurnManager.Reset()
  -> StoneController.ClearAll()
  -> IGameMode 생성 및 Initialize()
  -> GameManager.FireTurn()
  -> 현재 모드의 OnTurnStart()
```

착수 흐름은 다음과 같습니다.

```text
InputHandler.Update()
  -> 카메라 Raycast로 Board 레이어 히트
  -> StoneController.WorldToGrid()
  -> GameManager.OnBoardTapped(row, col)
  -> BoardManager.TryPlace()
  -> BoardManager.OnStonePlaced 이벤트
  -> StoneController.Place()가 돌 프리팹 생성
  -> WinChecker.CheckWin()
  -> 턴 전환 또는 게임 종료
```

AI 대전에서는 `AIPlayMode.OnTurnStart()`가 현재 턴이 AI인지 확인합니다. AI 턴이면 입력을 끄고 `Think()` 코루틴을 실행한 뒤, `OmokAI.GetBestMove()`가 고른 좌표를 다시 `GameManager.OnBoardTapped()`로 넣습니다. 즉, 사람과 AI 모두 최종 착수 처리는 같은 진입점을 공유합니다.

## 5. Core 시스템

### GameManager

파일: `Assets/Scripts/Core/GameManager.cs`

전체 게임 상태를 조율하는 중앙 매니저입니다.

주요 책임:

- 게임 시작과 종료
- 현재 모드 보관
- 보드/턴/돌/이펙트 초기화
- 착수 요청 처리
- 승리/무승부 판정
- 금수 메시지 표시
- 되돌리기 처리
- 입력 활성화/비활성화
- UI가 구독하는 이벤트 발행

주요 이벤트:

- `OnTurnChanged(Player)`
- `OnGameOver(Player)`
- `OnMoveMade(int row, int col, int player)`

주의할 점:

- `StartGame()`에서 `StopAllCoroutines()`를 호출해 이전 게임의 잔여 AI 코루틴을 정리합니다.
- `FireTurn()`은 바로 모드 처리를 하지 않고 다음 프레임에 `DelayedModeStart()`를 실행합니다. UI 갱신이 먼저 끝난 뒤 AI 코루틴이 시작되도록 하기 위한 구조입니다.
- 게임 종료 시 `SetInput(false)`로 입력을 막고 결과 UI를 띄웁니다.

### BoardManager

파일: `Assets/Scripts/Core/BoardManager.cs`

15x15 보드 데이터와 착수 기록을 관리합니다.

데이터 표현:

- `int[,] Board`
- 빈 칸: `0`
- 흑: `1`
- 백: `2`

주요 메서드:

- `Init()`: 보드와 히스토리 초기화
- `TryPlace(row, col, player)`: 착수 가능 여부 검사 후 착수
- `Undo(out row, out col, out player)`: 마지막 수 되돌리기
- `CheckWin()`, `GetWinLine()`, `IsFull()`
- `GetCopy()`: AI/룰 검사용 복사본 생성

흑돌 착수에서는 `RenjuRule.GetForbiddenType()`으로 금수를 먼저 검사합니다. 이때 원본 보드가 오염되지 않도록 복사본을 넘깁니다.

### TurnManager

파일: `Assets/Scripts/Core/TurnManager.cs`

현재 턴과 수 카운트를 관리합니다.

주의할 점:

- `Next()`에서 현재 플레이어를 바꾼 뒤 `MoveCount`를 증가시킵니다.
- `GameHUD.OnMoveMade()`는 `GameManager.Instance.Turn.MoveCount`를 표시하는데, 이벤트 발행 시점상 방금 둔 수가 반영되기 전일 수 있습니다. 수 카운트 표시가 어색하면 이 흐름을 먼저 확인하세요.

### InputHandler

파일: `Assets/Scripts/Core/InputHandler.cs`

마우스 클릭 또는 터치 종료 시점에 카메라 레이캐스트로 보드 좌표를 계산합니다.

필수 Inspector 연결:

- `_cam`: 게임 카메라
- `_boardMask`: `Board` 레이어
- `_gm`: `GameManager`
- `_stone`: `StoneController`

입력이 먹지 않으면 가장 먼저 확인할 것:

- 보드 오브젝트 레이어가 `Board`인지
- 카메라가 올바르게 연결되어 있는지
- `GameManager.SetInput(true)`가 호출되고 있는지
- `StoneController.WorldToGrid()`의 좌표 오프셋이 보드 렌더러와 맞는지

### WinChecker

파일: `Assets/Scripts/Core/WinChecker.cs`

4개 방향을 검사해 5목 이상이면 승리로 판단합니다.

검사 방향:

- 가로
- 세로
- 대각선 `\`
- 대각선 `/`

`GetWinLine()`은 승리 이펙트에 사용할 좌표 리스트를 반환합니다.

### RenjuRule

파일: `Assets/Scripts/Core/RenjuRule.cs`

흑돌 금수를 판정합니다.

지원하는 금수:

- `DoubleThree`: 3-3
- `DoubleFour`: 4-4
- `Overline`: 장목

`GetForbiddenType()`은 전달받은 보드에 임시로 흑돌을 놓고 검사한 뒤 `finally`에서 다시 빈 칸으로 복구합니다. 그래서 호출하는 쪽에서도 원본 대신 복사본을 넘기는 현재 패턴을 유지하는 것이 좋습니다.

## 6. 게임 모드 구조

게임 모드는 `IGameMode` 인터페이스로 분리되어 있습니다.

파일:

- `Assets/Scripts/Modes/IGameMode.cs`
- `Assets/Scripts/Modes/SinglePlayMode.cs`
- `Assets/Scripts/Modes/AIPlayMode.cs`
- `Assets/Scripts/Modes/MultiPlayMode.cs`

인터페이스:

```csharp
public interface IGameMode
{
    void Initialize(GameManager gm);
    void OnTurnStart(Player current);
    void OnStonePlace(int row, int col, Player player);
    void OnGameEnd(Player winner);
}
```

현재 상태:

- `SinglePlayMode`: 매 턴 입력 허용
- `AIPlayMode`: 사람 턴에는 입력 허용, AI 턴에는 입력 차단 후 AI 코루틴 실행
- `MultiPlayMode`: 아직 네트워크 구현 전이며 싱글과 거의 동일하게 동작

새 모드를 추가할 때는 `IGameMode` 구현체를 만들고 `GameManager.StartGame()`의 `mode switch`에 연결하면 됩니다.

## 7. AI 시스템

파일:

- `Assets/Scripts/AI/OmokAI.cs`
- `Assets/Scripts/AI/BoardEvaluator.cs`

AI 처리 순서:

```text
AIPlayMode.Think()
  -> BoardManager.GetCopy()
  -> OmokAI.GetBestMove(board)
  -> 후보 수 생성
  -> 흑이면 금수 후보 제거
  -> 즉시 승리/방어 수 확인
  -> Alpha-Beta 탐색
  -> 최고 점수 좌표 반환
```

난이도는 `OmokAI.Configs` 배열에 정의되어 있습니다.

- 쉬움: 낮은 깊이, 후보 적음, 실수 확률 있음
- 보통: 깊이 증가, 즉시 승리/방어 사용
- 어려움: 더 깊은 탐색, 실수 확률 없음

`BoardEvaluator`는 공격 점수에서 상대 점수에 수비 가중치를 곱한 값을 뺍니다.

```text
평가 점수 = AI 패턴 점수 - 상대 패턴 점수 * defenseWeight
```

AI를 수정할 때는 다음 순서로 보면 좋습니다.

1. 후보 수가 적절한지 `GetCandidates()` 확인
2. 즉시 승리/방어 로직이 원하는 순서인지 확인
3. 패턴 점수가 부족하면 `BoardEvaluator.EvalLine()`의 점수표 조정
4. 난이도 밸런스는 `Configs`에서 조정

## 8. 렌더링과 좌표계

파일:

- `Assets/Scripts/Rendering/BoadrRenderer.cs`
- `Assets/Scripts/Core/StoneController.cs`
- `Assets/Scripts/Rendering/EffectManager.cs`
- `Assets/Scripts/Rendering/ForbiddenMarker.cs`
- `Assets/Scripts/Rendering/CameraController.cs`

주의: 파일명은 `BoadrRenderer.cs`로 오타가 있지만 클래스명은 `BoardRenderer3D`입니다. Unity에서는 파일명과 클래스명이 다르면 관리가 헷갈릴 수 있으므로 추후 정리를 권장합니다.

### BoardRenderer3D

런타임에 보드 표면, 격자선, 화점을 생성합니다. 보드 표면에는 `Board` 레이어를 설정해 `InputHandler`의 레이캐스트 대상이 되게 합니다.

### StoneController

보드 이벤트를 구독해 돌을 생성/제거합니다.

중요 메서드:

- `GridToWorld(row, col)`
- `WorldToGrid(worldPosition)`
- `ClearAll()`

좌표 기본값:

- `_cell = 1f`
- `_ofsX = -7f`
- `_ofsZ = -7f`

15x15 보드는 0부터 14까지이므로, 중앙 `(7, 7)`이 월드 좌표 `(0, y, 0)` 근처가 됩니다.

### EffectManager

승리 좌표 리스트를 받아 첫 좌표와 마지막 좌표 사이에 파티클 이펙트를 배치합니다. 승자 색에 따라 흑/백 이펙트 프리팹을 선택합니다.

### ForbiddenMarker

금수 위치에 X 표시를 잠깐 생성합니다. `BoardManager.OnForbiddenMove` 이벤트를 구독합니다.

### CameraController

모바일 핀치 줌과 한 손가락 팬을 처리합니다. 기본 카메라 위치와 회전도 여기서 초기화합니다.

## 9. UI 시스템

파일:

- `Assets/Scripts/UI/UIManager.cs`
- `Assets/Scripts/UI/MainMenuUI.cs`
- `Assets/Scripts/UI/GameHUD.cs`
- `Assets/Scripts/UI/ResultUI.cs`
- `Assets/Scripts/UI/PauseUI.cs`
- `Assets/Scripts/UI/SettingsUI.cs`
- `Assets/Scripts/UI/ToastUI.cs`
- `Assets/Scripts/UI/CardSlotUI.cs`

### UIManager

메인 메뉴, 게임 HUD, 결과 패널, 일시정지 패널을 스택 방식으로 관리합니다.

- `SwitchTo()`: 기존 스택을 비우고 특정 패널만 표시
- `Push()`: 현재 패널 위에 새 패널 표시
- `HideTop()`: 최상단 패널 닫고 이전 패널 복귀

### MainMenuUI

메인 메뉴 안의 세부 패널 전환과 게임 시작 버튼을 처리합니다.

흐름:

```text
시작
  -> 모드 선택
  -> Single: 바로 시작
  -> AI: 난이도 선택 -> 색 선택 -> 시작
  -> Multi: 현재 준비 중 토스트
```

### GameHUD

턴 표시, 타이머, 수 카운트, 되돌리기, 일시정지를 담당합니다.

타이머:

- AI 모드: 60초
- Multi 모드: 15초
- Single 모드: 15초

시간 초과 시 `GameManager.OnTimeOut()`을 호출해 턴을 넘깁니다.

### ResultUI

게임 종료 후 결과 표시, 재대결, 메인 메뉴 복귀, 새 게임 시작 흐름을 담당합니다.

현재 `Show()`에서 항상 `AudioManager.PlayWin()`을 호출합니다. 패배/무승부 사운드를 구분하려면 여기부터 수정하면 됩니다.

### PauseUI

패널이 켜지면 `Time.timeScale = 0f`, 재개/재시작/메인 메뉴 이동 시 `1f`로 복구합니다.

### SettingsUI

BGM/SFX 볼륨과 뮤트를 `AudioManager`와 연결합니다. 설정값은 `PlayerPrefs`에 저장됩니다.

### ToastUI

정적 메서드 `ToastUI.Show(message)`로 짧은 메시지를 표시합니다. 금수, 준비 중 기능 안내 등에 사용됩니다.

## 10. 오디오

파일: `Assets/Scripts/Core/AudioManager.cs`

싱글톤으로 BGM과 SFX를 관리합니다.

주요 기능:

- 메뉴 BGM 재생
- 게임 BGM 재생
- 착수/승리/패배/금수/버튼 SFX 재생
- 볼륨과 뮤트 상태 저장

`Awake()`에서 `PlayMenuBGM()`을 호출합니다. 게임 시작 시 `GameManager.StartGame()`에서 `PlayGameBGM()`을 호출합니다.

## 11. 카드 시스템

파일:

- `Assets/Scripts/Cards/ICard.cs`
- `Assets/Scripts/Cards/CardBase.cs`
- `Assets/Scripts/Cards/CardManager.cs`
- `Assets/Scripts/UI/CardSlotUI.cs`

현재는 카드 시스템의 기초 구조만 있습니다.

구조:

- `CardBase`: `ScriptableObject` 기반 카드 추상 클래스
- `ICard`: 카드 이름, 설명, 사용 가능 여부, 실행 인터페이스
- `CardManager`: 덱 템플릿에서 랜덤으로 손패 지급/사용
- `CardSlotUI`: 카드 한 장의 UI 표시와 클릭 연결

아직 실제 카드 구현체는 보이지 않습니다. 새 카드를 만들려면 `CardBase`를 상속한 ScriptableObject 클래스를 만들고, `CanUse()`와 `Execute()`를 구현해야 합니다. Unity 에디터에서 에셋으로 만들려면 새 클래스에 `[CreateAssetMenu]`를 추가하는 것이 좋습니다.

## 12. 백엔드

파일:

- `Assets/Scripts/Backend/BackendManager.cs`
- `Assets/Scripts/Backend/BackendLogin.cs`
- `Assets/TheBackend/`

The Backend SDK 초기화와 커스텀 로그인/닉네임 변경 테스트 코드가 들어 있습니다.

현재 주의점:

- 일부 주석과 로그 문자열이 인코딩 깨짐 상태입니다.
- `BackendManager.Start()`에서 `Test()`를 호출하고, 고정 계정 `user1 / 1234`로 로그인합니다.
- 운영용 로그인 플로우라기보다 SDK 연동 테스트 코드에 가깝습니다.

백엔드 기능을 실제 서비스 플로우로 확장하려면 먼저 인코딩 깨짐을 정리하고, 테스트 계정 자동 로그인 코드를 제거한 뒤 UI 입력 기반으로 연결해야 합니다.

## 13. 자주 하는 수정 작업별 진입점

### 착수 규칙을 바꾸고 싶을 때

먼저 볼 파일:

- `BoardManager.cs`
- `RenjuRule.cs`
- `WinChecker.cs`

금수는 흑돌에만 적용됩니다. 백돌에도 특수 규칙을 넣으려면 `BoardManager.TryPlace()`의 `player == 1` 조건을 중심으로 수정합니다.

### 승리 판정을 바꾸고 싶을 때

먼저 볼 파일:

- `WinChecker.cs`
- `BoardManager.cs`
- `EffectManager.cs`
- `ResultUI.cs`

승리 조건 변경은 `WinChecker.CheckWin()`이 핵심입니다. 승리 이펙트 위치가 이상하면 `GetWinLine()` 반환 순서와 `EffectManager.ShowWinLine()`을 같이 봅니다.

### AI 난이도를 조정하고 싶을 때

먼저 볼 파일:

- `OmokAI.cs`
- `BoardEvaluator.cs`

빠르게 밸런스를 바꾸려면 `OmokAI.Configs`의 `Depth`, `CandidateRange`, `MaxCandidates`, `DefenseWeight`, `BlunderChance`를 조정합니다.

### UI 패널 흐름을 바꾸고 싶을 때

먼저 볼 파일:

- `UIManager.cs`
- `MainMenuUI.cs`
- `ResultUI.cs`
- `PauseUI.cs`

버튼 클릭은 대부분 Inspector의 OnClick에 public 메서드가 연결되는 구조입니다. 코드만 고쳤는데 버튼이 동작하지 않으면 Inspector 연결을 확인해야 합니다.

### 입력이 안 될 때

먼저 볼 파일:

- `InputHandler.cs`
- `BoardRenderer3D.cs`
- `StoneController.cs`
- `GameManager.cs`

확인 순서:

1. `GameManager.SetInput(true)`가 호출되는지
2. 카메라와 보드 마스크가 Inspector에 연결되어 있는지
3. 보드 표면 레이어가 `Board`인지
4. 보드 Collider가 존재하는지
5. `WorldToGrid()` 변환 결과가 0~14 범위인지

### 사운드나 설정을 바꾸고 싶을 때

먼저 볼 파일:

- `AudioManager.cs`
- `SettingsUI.cs`
- `ButtonSFX.cs`

`AudioManager`가 싱글톤이므로 씬에 중복 배치되면 기존 인스턴스가 새 인스턴스를 파괴합니다.

## 14. 이벤트 중심 연결

이 프로젝트는 직접 호출과 이벤트 구독을 섞어서 사용합니다.

중요 이벤트:

```text
BoardManager.OnStonePlaced
  -> StoneController.Place()

BoardManager.OnStoneRemoved
  -> StoneController.Remove()

BoardManager.OnForbiddenMove
  -> GameManager.OnForbiddenMove()
  -> ForbiddenMarker.ShowMarker()

GameManager.OnTurnChanged
  -> GameHUD.OnTurnChanged()

GameManager.OnMoveMade
  -> GameHUD.OnMoveMade()

GameManager.OnGameOver
  -> GameHUD.OnGameOver()
  -> ResultUI.Show()는 GameManager.EndGame()에서 직접 호출
```

새 기능을 붙일 때는 `GameManager`를 더 비대하게 만들기보다, 이미 있는 이벤트를 구독할 수 있는지 먼저 확인하세요.

## 15. 현재 코드에서 눈여겨볼 위험 요소

아래 항목은 당장 문서 작성 중 발견된 유지보수 포인트입니다.

- `Assets/Scripts/Rendering/BoadrRenderer.cs` 파일명 오타가 있습니다. 클래스명 `BoardRenderer3D`와 맞지 않습니다.
- `BackendManager.cs`, `BackendLogin.cs`, `README.md`에 인코딩 깨짐이 있습니다.
- `BackendManager.Start()`가 테스트 로그인 코드를 자동 실행합니다.
- `SceneBootstrapper.cs`는 전체가 주석 처리되어 있어 현재 미사용으로 보입니다.
- `CardManager.DrawRandom()`은 `_deckTemplate`이 비어 있으면 예외가 납니다.
- `GameHUD.OnMoveMade()`의 수 표시가 `TurnManager.Next()` 이전 값으로 보일 가능성이 있습니다.
- `SettingsUI.RefreshBGM()`과 `RefreshSFX()` 안에 색상/슬라이더 갱신 코드가 중복으로 들어 있습니다.
- `ResultUI.Show()`는 승자/패자/무승부와 관계없이 승리 사운드만 재생합니다.
- `OmokAI.HasNeighbor()`는 현재 사용되지 않습니다.
- `MultiPlayMode`는 네트워크 구현 전 placeholder입니다.

## 16. 신입 개발자 추천 학습 순서

처음에는 아래 순서로 읽는 것을 추천합니다.

1. `GameState.cs`: enum 값으로 전체 도메인 용어 파악
2. `GameManager.cs`: 게임 시작, 착수, 종료 흐름 파악
3. `BoardManager.cs`: 보드 데이터와 착수 검증 파악
4. `InputHandler.cs`, `StoneController.cs`: 화면 클릭이 돌 생성으로 이어지는 과정 파악
5. `WinChecker.cs`, `RenjuRule.cs`: 오목/렌주 규칙 파악
6. `SinglePlayMode.cs`, `AIPlayMode.cs`: 모드별 턴 시작 처리 파악
7. `OmokAI.cs`, `BoardEvaluator.cs`: AI 구조 파악
8. `UIManager.cs`, `MainMenuUI.cs`, `GameHUD.cs`, `ResultUI.cs`: UI 흐름 파악
9. `AudioManager.cs`, `SettingsUI.cs`: 설정과 사운드 파악
10. `Cards/`, `Backend/`: 확장 예정 영역 파악

## 17. 기능 추가 예시

### 예시 1: 새 AI 난이도 추가

1. `OmokAI.Configs`에 새 난이도 설정을 추가합니다.
2. `MainMenuUI`에 새 난이도 버튼 메서드를 추가합니다.
3. Unity Inspector에서 새 버튼 OnClick에 메서드를 연결합니다.
4. 어려운 난이도일수록 `Depth`와 `MaxCandidates`가 커지므로 모바일 성능을 꼭 확인합니다.

### 예시 2: 새 카드 추가

1. `CardBase`를 상속하는 새 클래스를 만듭니다.
2. `[CreateAssetMenu]`를 붙여 Unity 에디터에서 카드 에셋을 만들 수 있게 합니다.
3. `CanUse()`에서 현재 보드/턴 상태를 검사합니다.
4. `Execute()`에서 실제 효과를 적용합니다.
5. 만든 카드 에셋을 `CardManager._deckTemplate`에 등록합니다.
6. 손패 UI 갱신 로직이 필요하면 `CardSlotUI`를 사용하는 상위 UI를 추가합니다.

### 예시 3: 멀티플레이 구현 시작

1. `MultiPlayMode`의 책임을 먼저 정의합니다.
2. 로컬 입력 허용 조건을 “내 턴일 때만”으로 바꿉니다.
3. 착수 송수신 프로토콜을 정합니다.
4. 원격 착수도 최종적으로는 `GameManager.OnBoardTapped()` 또는 그에 준하는 공통 진입점을 타게 만듭니다.
5. 금수/승패 판정은 양쪽 클라이언트에서 같은 결과가 나와야 하므로 보드 동기화 전략을 먼저 고정합니다.

## 18. 작업 전 체크리스트

새 작업을 시작하기 전에는 다음을 확인하세요.

- 어떤 씬에서 테스트할 것인지 정했는가
- 해당 기능이 Inspector 연결에 의존하는가
- 싱글/AI/멀티 모드 중 어떤 모드에 영향을 주는가
- `GameManager`, `BoardManager`, `TurnManager` 중 어디까지 건드리는가
- 이벤트 구독을 추가했다면 해제도 같이 했는가
- 코루틴을 시작했다면 게임 종료/재시작 때 정리되는가
- 모바일 터치와 마우스 입력 모두 고려했는가
- AI가 흑일 때 금수 필터링이 깨지지 않는가

## 19. 간단 용어집

- 착수: 보드에 돌을 놓는 행위
- 금수: 흑돌이 둘 수 없는 자리
- 장목: 6목 이상이 되는 수
- 3-3: 열린 3이 동시에 두 개 생기는 수
- 4-4: 4가 동시에 두 개 생기는 수
- 휴리스틱: 완전 탐색 대신 현재 보드의 유리함을 점수로 추정하는 방법
- Alpha-Beta: Minimax 탐색에서 불필요한 가지를 잘라내는 최적화
- Zobrist Hash: 보드 상태를 빠르게 식별하기 위한 해시 기법

## 20. 결론

이 프로젝트의 중심축은 `GameManager -> BoardManager -> StoneController/UI` 이벤트 흐름입니다. 신입 개발자는 먼저 한 수가 입력되어 보드에 저장되고, 돌이 렌더링되고, 승패가 판정되고, UI가 갱신되는 길을 따라가면 전체 구조를 가장 빠르게 이해할 수 있습니다.

확장 예정 영역은 카드, 멀티플레이, 백엔드입니다. 이 세 영역은 현재 뼈대 또는 테스트 코드 성격이 강하므로, 기능을 본격적으로 붙이기 전에 책임 범위와 데이터 흐름을 먼저 정리하는 것이 좋습니다.
