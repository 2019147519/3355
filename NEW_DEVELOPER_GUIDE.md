# 3355 Unity 프로젝트 개발 가이드

최종 갱신일: 2026-05-12

이 문서는 새로 프로젝트에 들어온 개발자가 현재 코드 구조를 빠르게 파악하고, 어디를 수정해야 하는지 판단할 수 있도록 정리한 개발 문서입니다.

## 1. 프로젝트 개요

`3355`는 Unity 기반 15x15 오목 게임입니다. 현재 구현된 주요 기능은 다음과 같습니다.

- 싱글 플레이: 한 기기에서 흑돌/백돌을 번갈아 착수
- AI 플레이: Min-Max 기반 AI와 대전
- 온라인 매치: 뒤끝 Match SDK 기반 1:1 실시간 오목 대전
- 로그인/회원가입: 뒤끝 Base SDK 커스텀 계정 사용
- 랭킹: 온라인 대전 점수를 뒤끝 리더보드에 기록
- UI: 메인 메뉴, 게임 HUD, 결과/재대결, 매칭 상태 패널, 리더보드 패널

기본 환경:

- Unity 버전: `6000.3.13f1`
- 보드 크기: 15x15
- 플레이어 값: `None = 0`, `Black = 1`, `White = 2`
- 흑돌 금수: 3-3, 4-4, 장목
- 주요 SDK/패키지: UGUI, TextMeshPro, Universal RP, Input System, The Backend SDK

## 2. 폴더 구조

```text
Assets/
  Scenes/                 LoginScene, GameScene
  Scripts/
    Core/                 게임 진행, 보드, 턴, 입력, 승패, 금수
    Modes/                Single, AI, Multi 게임 모드 전략
    AI/                   오목 AI와 보드 평가
    Backend/              뒤끝 로그인, 온라인 매치, 랭킹
    UI/                   메뉴, HUD, 결과, 리더보드, 매칭 상태 UI
    Rendering/            보드/돌/이펙트/카메라 렌더링
    Cards/                카드 시스템 기반 구조
  TheBackend/             뒤끝 SDK 관련 파일
Packages/                 Unity 패키지 매니페스트
ProjectSettings/          Unity 프로젝트 설정
```

`Assets/TextMesh Pro/Examples & Extras` 아래 파일은 TextMeshPro 예제입니다. 게임 로직 분석 시에는 대부분 무시해도 됩니다.

## 3. 실행 흐름

로그인부터 게임 시작까지의 기본 흐름입니다.

```text
LoginScene
  -> BackendLoginUI
  -> BackendManager.Initialize()
  -> Login 또는 SignUp
  -> 로그인 성공 시 GameScene 로드

GameScene
  -> MainMenuUI
  -> GameManager.StartGame(mode)
  -> BoardManager.Init()
  -> TurnManager.Reset()
  -> StoneController.ClearAll()
  -> IGameMode.Initialize()
  -> GameManager.FireTurn()
```

착수 흐름은 모든 모드에서 `GameManager.OnBoardTapped()`를 중심으로 처리됩니다.

```text
InputHandler
  -> 보드 Raycast
  -> StoneController.WorldToGrid()
  -> GameManager.OnBoardTapped(row, col)
  -> BoardManager.TryPlace()
  -> OnMoveMade 이벤트
  -> IGameMode.OnStonePlace()
  -> 승리/무승부 검사
  -> 턴 전환
```

## 4. Core 시스템

### GameManager

파일: `Assets/Scripts/Core/GameManager.cs`

전체 게임 흐름의 중심입니다.

주요 역할:

- 게임 시작/종료
- 현재 게임 모드 관리
- 보드, 턴, 돌, 이펙트 초기화
- 착수 처리
- 승리/무승부 판정
- 금수 메시지 처리
- 입력 활성화/비활성화
- UI가 구독하는 이벤트 발행

주요 이벤트:

- `OnTurnChanged(Player)`
- `OnMoveMade(int row, int col, int player)`
- `OnGameOver(Player)`

AI HUD 표시를 위해 현재 AI 난이도와 AI 색상도 읽기 전용 프로퍼티로 제공합니다.

- `AIDifficulty`
- `AIColor`

### BoardManager

파일: `Assets/Scripts/Core/BoardManager.cs`

15x15 보드 데이터와 착수 이력을 관리합니다.

주요 역할:

- `int[,] Board` 관리
- 빈 칸, 중복 착수, 금수 검사
- 착수/되돌리기
- 승리 검사 위임
- AI와 금수 검사에서 사용할 보드 복사본 제공

흑돌 착수 시 `RenjuRule`을 통해 3-3, 4-4, 장목 금수를 검사합니다.

### TurnManager

파일: `Assets/Scripts/Core/TurnManager.cs`

현재 턴과 수 카운트를 관리합니다.

주의할 점:

- `Next()`가 호출되면 현재 플레이어가 바뀌고 `MoveCount`가 증가합니다.
- HUD의 수 카운트 표시가 어색하면 `GameManager.OnMoveMade` 호출 시점과 `TurnManager.Next()` 호출 시점을 함께 확인해야 합니다.

### InputHandler

파일: `Assets/Scripts/Core/InputHandler.cs`

마우스 클릭과 터치 입력을 보드 좌표로 변환합니다.

입력이 되지 않을 때 확인할 것:

- `GameManager.SetInput(true)`가 호출되는지
- 보드 오브젝트 레이어가 `_boardMask`와 맞는지
- 카메라와 보드 Collider가 연결되어 있는지
- `StoneController.WorldToGrid()` 결과가 0~14 범위인지

### WinChecker / RenjuRule

파일:

- `Assets/Scripts/Core/WinChecker.cs`
- `Assets/Scripts/Core/RenjuRule.cs`

`WinChecker`는 가로, 세로, 대각선 2방향을 검사해 5목 이상을 승리로 판정합니다.

`RenjuRule`은 흑돌 금수를 판정합니다.

- 3-3
- 4-4
- 장목

## 5. 게임 모드 구조

게임 모드는 `IGameMode` 인터페이스로 분리되어 있습니다.

파일:

- `Assets/Scripts/Modes/IGameMode.cs`
- `Assets/Scripts/Modes/SinglePlayMode.cs`
- `Assets/Scripts/Modes/AIPlayMode.cs`
- `Assets/Scripts/Modes/MultiPlayMode.cs`

```csharp
public interface IGameMode
{
    void Initialize(GameManager gm);
    void OnTurnStart(Player current);
    void OnStonePlace(int row, int col, Player player);
    void OnGameEnd(Player winner);
}
```

### SinglePlayMode

로컬 2인 대전입니다. 매 턴 입력을 허용합니다.

### AIPlayMode

사람 턴에는 입력을 켜고, AI 턴에는 입력을 끈 뒤 `OmokAI.GetBestMove()`로 착수 좌표를 계산합니다.

AI도 최종적으로 `GameManager.OnBoardTapped()`를 호출하기 때문에 사람과 AI의 착수 처리는 같은 경로를 사용합니다.

### MultiPlayMode

뒤끝 온라인 매치 모드입니다.

주요 규칙:

- `OnlineMatchManager.IsInGameRoom`이 true여야 입력 가능
- 현재 턴이 `OnlineMatchManager.LocalPlayer`일 때만 입력 가능
- 내가 둔 수만 `OnlineMatchManager.SendMove()`로 전송
- 상대 수신 착수는 `IsApplyingRemoteMove`로 다시 송신되지 않게 방지
- 온라인 대전에서는 무르기 사용 불가

## 6. AI 시스템

파일:

- `Assets/Scripts/AI/OmokAI.cs`
- `Assets/Scripts/AI/BoardEvaluator.cs`

AI 흐름:

```text
AIPlayMode.Think()
  -> BoardManager.GetCopy()
  -> OmokAI.GetBestMove()
  -> 후보 수 생성
  -> 금수 후보 제거
  -> 즉시 승리/방어 검사
  -> Alpha-Beta Min-Max 탐색
  -> 최고 점수 좌표 반환
  -> GameManager.OnBoardTapped()
```

### Min-Max 설명

가능한 착수를 트리 구조로 펼쳐 여러 수 앞의 판세를 예측하고, 마지막 노드를 평가 함수로 점수화한 뒤 AI는 최대 점수, 상대는 최소 점수를 선택한다고 가정해 최적의 수를 고릅니다.

### 평가 함수

`BoardEvaluator.Evaluate()`는 AI의 공격 점수에서 상대 점수에 수비 가중치를 곱한 값을 뺍니다.

```text
평가 점수 = AI 패턴 점수 - 상대 패턴 점수 * defenseWeight
```

패턴 점수는 연속된 돌 개수와 양쪽이 열려 있는지에 따라 계산됩니다.

- 5목 이상: 매우 큰 점수
- 열린 4
- 막힌 4
- 열린 3
- 막힌 3
- 열린 2
- 막힌 2

### 최적화 기법

- 후보 수 제한: 이미 놓인 돌 주변 빈 칸만 후보로 사용
- 후보 정렬: 공격/방어 휴리스틱 점수가 높은 후보를 먼저 탐색
- 즉시 승리/방어: 명확한 승리 수와 방어 수는 깊은 탐색 전에 처리
- Alpha-Beta 가지치기: 결과에 영향 없는 분기를 건너뜀
- 전이 테이블: 이미 평가한 보드 상태를 캐싱
- Zobrist 해시: 보드 상태를 빠르게 캐시 키로 변환
- 금수 필터링: 흑돌이 둘 수 없는 수를 후보에서 제외
- 난이도 설정: 탐색 깊이, 후보 수, 수비 가중치, 실수 확률 조절

난이도는 `OmokAI.Configs`에서 조정합니다.

| 난이도 | Depth | CandidateRange | MaxCandidates | DefenseWeight | BlunderChance |
| --- | ---: | ---: | ---: | ---: | ---: |
| 초급 | 2 | 1 | 8 | 0.8 | 0.3 |
| 중급 | 4 | 2 | 15 | 1.5 | 0.08 |
| 고급 | 5 | 2 | 20 | 1.8 | 0 |

## 7. 뒤끝 로그인

파일:

- `Assets/Scripts/Backend/BackendManager.cs`
- `Assets/Scripts/Backend/BackendLoginUI.cs`
- `Assets/Scripts/Backend/BackendRuntimeBootstrap.cs`

로그인 방식:

- 로그인: 아이디, 비밀번호만 입력
- 회원가입: 아이디, 비밀번호, 비밀번호 확인, 닉네임 입력
- 회원가입 성공 시 `Backend.BMember.UpdateNickname()`으로 닉네임 설정
- 로그인 성공 시 뒤끝에 저장된 닉네임을 읽어 `CurrentNickname`에 저장
- 마지막 닉네임은 `PlayerPrefs`의 `BackendLastNickname`에도 저장

`BackendManager.Update()`에서 `Backend.Match.Poll()`을 주기적으로 호출합니다. 뒤끝 Match 이벤트 수신에 필요합니다.

## 8. 온라인 매치

파일:

- `Assets/Scripts/Backend/OnlineMatchManager.cs`
- `Assets/Scripts/Modes/MultiPlayMode.cs`
- `Assets/Scripts/UI/OnlineMatchStatusUI.cs`
- `Assets/Scripts/UI/ResultUI.cs`

매칭 흐름:

```text
MainMenuUI.OnModeMulti()
  -> OnlineMatchManager.StartQuickMatch()
  -> JoinMatchMakingServer
  -> CreateMatchRoom
  -> RequestMatchMaking
  -> JoinGameServer
  -> JoinGameRoom
  -> OnMatchInGameStart
  -> GameManager.StartGame(GameMode.Multi)
```

매칭 중에는 `OnlineMatchStatusUI` 패널을 띄우고 메시지를 표시합니다. 취소 버튼을 누르면 `OnlineMatchManager.CancelMatchmaking()`으로 매칭을 중단합니다.

### 착수 동기화

내가 둔 수는 `OnlineOmokPacket`으로 직렬화되어 `Backend.Match.SendDataToInGameRoom()`으로 전송됩니다.

상대 착수 수신 시:

```text
OnMatchRelay()
  -> JsonUtility.FromJson<OnlineOmokPacket>()
  -> IsApplyingRemoteMove = true
  -> GameManager.OnBoardTapped(row, col)
  -> IsApplyingRemoteMove = false
```

### 흑돌/백돌 결정

온라인 대전 시작 시 `CurrentColorSeed`와 방 안의 닉네임 목록을 이용해 흑돌/백돌을 결정합니다.

- `BlackNickname`
- `WhiteNickname`
- `LocalPlayer`

재대결 시에도 양쪽 클라이언트가 보낸 seed를 XOR해서 다시 흑백을 결정합니다.

### 재대결

게임 종료 후 `ResultUI`가 재대결 화면을 표시합니다.

- 양쪽 모두 O: 같은 상대와 재대결
- 한쪽이라도 X: 온라인 세션 정리 후 PostGameView
- 내가 O를 눌렀는데 상대가 나감: “상대방이 나갔습니다.” 표시 후 PostGameView
- 상대 응답 없음: 15초 후 세션 정리

온라인 세션 정리는 `OnlineMatchManager.FinishOnlineSession()`에서 처리합니다.

## 9. 랭킹 시스템

파일:

- `Assets/Scripts/Backend/BackendWinRankingManager.cs`
- `Assets/Scripts/UI/RankingPanelUI.cs`
- `Assets/Scripts/UI/RankingRowUI.cs`
- `Assets/Scripts/UI/ResultUI.cs`

점수 규칙:

- 승리: `score + 1`, `wins + 1`
- 패배: `score - 1`, `losses + 1`
- 점수 최소값: `0`
- 무승부: 현재 점수 변화 없음

온라인 게임 종료 시 `ResultUI.Show()`에서 승패에 따라 다음 메서드를 호출합니다.

- `BackendWinRankingManager.ReportOnlineWin()`
- `BackendWinRankingManager.ReportOnlineLoss()`

리더보드 UI:

- `RankingPanelUI`가 뒤끝 리더보드 목록을 불러와 row prefab을 생성
- 일반 row는 순위, 닉네임, 점수를 표시
- 하단 `MyRankRow`는 현재 UI 기준으로 순위와 점수만 표시
- 닫기 버튼은 패널을 비활성화

## 10. Game HUD

파일: `Assets/Scripts/UI/GameHUD.cs`

표시 항목:

- 현재 턴
- 흑돌/백돌 인디케이터
- 흑돌/백돌 플레이어명
- 타이머
- 수 카운트
- 무르기 버튼
- 일시정지 버튼

플레이어명 표시 규칙:

- 싱글: `흑돌`, `백돌`
- 멀티: 온라인 매치에서 결정된 흑돌/백돌 닉네임
- AI: 플레이어 쪽은 닉네임, AI 쪽은 `초급/중급/고급 인공지능`

타이머:

- AI 모드: 60초
- Multi 모드: 15초
- Single 모드: 15초

시간 초과 시 `GameManager.OnTimeOut()`을 호출해 턴을 넘깁니다.

## 11. UI 시스템

주요 파일:

- `Assets/Scripts/UI/UIManager.cs`
- `Assets/Scripts/UI/MainMenuUI.cs`
- `Assets/Scripts/UI/GameHUD.cs`
- `Assets/Scripts/UI/ResultUI.cs`
- `Assets/Scripts/UI/PauseUI.cs`
- `Assets/Scripts/UI/SettingsUI.cs`
- `Assets/Scripts/UI/ToastUI.cs`
- `Assets/Scripts/UI/OnlineMatchStatusUI.cs`
- `Assets/Scripts/UI/RankingPanelUI.cs`

`UIManager`는 메인 메뉴, 게임 HUD, 결과 패널, 일시정지 패널을 stack 방식으로 관리합니다.

`MainMenuUI`는 모드 선택, AI 난이도 선택, AI 색상 선택, 온라인 매칭 시작을 담당합니다.

`ResultUI`는 결과 표시, 재대결, PostGameView, 온라인 세션 종료, 랭킹 갱신을 연결합니다.

## 12. 렌더링과 좌표계

주요 파일:

- `Assets/Scripts/Rendering/BoadrRenderer.cs`
- `Assets/Scripts/Core/StoneController.cs`
- `Assets/Scripts/Rendering/EffectManager.cs`
- `Assets/Scripts/Rendering/ForbiddenMarker.cs`
- `Assets/Scripts/Rendering/CameraController.cs`

주의:

- 파일명이 `BoadrRenderer.cs`로 오타가 있습니다.
- 클래스명은 `BoardRenderer3D`입니다.
- Unity에서 파일명과 클래스명이 달라도 동작할 수 있지만, 유지보수 측면에서는 정리 대상입니다.

`StoneController`는 보드 좌표와 월드 좌표 변환을 담당합니다.

기본 좌표:

- `_cell = 1f`
- `_ofsX = -7f`
- `_ofsZ = -7f`
- 보드 중심 `(7, 7)`은 월드 좌표 `(0, y, 0)` 근처

## 13. 오디오와 설정

파일:

- `Assets/Scripts/Core/AudioManager.cs`
- `Assets/Scripts/UI/SettingsUI.cs`
- `Assets/Scripts/UI/ButtonSFX.cs`

`AudioManager`는 싱글톤으로 BGM과 SFX를 관리합니다.

주요 기능:

- 메뉴 BGM
- 게임 BGM
- 착수, 승리, 패배, 금수, 버튼 효과음
- BGM/SFX 볼륨과 mute 상태 저장

`SettingsUI`는 `PlayerPrefs`에 설정값을 저장합니다.

## 14. 카드 시스템

파일:

- `Assets/Scripts/Cards/ICard.cs`
- `Assets/Scripts/Cards/CardBase.cs`
- `Assets/Scripts/Cards/CardManager.cs`
- `Assets/Scripts/UI/CardSlotUI.cs`

현재는 카드 시스템의 기반 구조만 있습니다.

- `CardBase`: ScriptableObject 기반 카드 추상 클래스
- `ICard`: 카드 인터페이스
- `CardManager`: 덱에서 랜덤 카드 지급/사용
- `CardSlotUI`: 카드 슬롯 표시와 클릭 처리

실제 카드 기능을 추가하려면 `CardBase`를 상속한 ScriptableObject를 만들고 `CanUse()`, `Execute()`를 구현해야 합니다.

## 15. 뒤끝 콘솔 설정 요약

온라인 매치:

- Match 모드: `OneOnOne`
- MatchType: 코드 기준 `Random`
- 인원: 2명
- `OnlineMatchManager._matchCardInDate`에 매치 카드 inDate를 넣거나, 비워두면 코드가 `GetMatchList()`로 OneOnOne 매치 카드를 찾음

랭킹:

- 게임 정보 테이블: `omok_rank`
- 컬럼: `score`, `wins`, `losses`, `nickname`
- 유저 리더보드 제목: `OmokWins`
- 리더보드 점수 컬럼: `score`
- 정렬: 내림차순

자세한 설정은 `BACKND_ONLINE_MATCH_GUIDE.md`, `BACKND_RANKING_GUIDE.md`를 참고하세요.

## 16. 자주 수정하는 작업별 진입점

AI 난이도 조정:

- `Assets/Scripts/AI/OmokAI.cs`
- `Configs`의 `Depth`, `CandidateRange`, `MaxCandidates`, `DefenseWeight`, `BlunderChance` 수정

AI 평가 기준 조정:

- `Assets/Scripts/AI/BoardEvaluator.cs`
- `EvalLine()`의 패턴 점수 조정

온라인 착수 동기화 수정:

- `Assets/Scripts/Backend/OnlineMatchManager.cs`
- `Assets/Scripts/Modes/MultiPlayMode.cs`

재대결 흐름 수정:

- `Assets/Scripts/UI/ResultUI.cs`
- `Assets/Scripts/Backend/OnlineMatchManager.cs`

랭킹 점수 규칙 수정:

- `Assets/Scripts/Backend/BackendWinRankingManager.cs`
- `Assets/Scripts/UI/ResultUI.cs`

리더보드 UI 수정:

- `Assets/Scripts/UI/RankingPanelUI.cs`
- `Assets/Scripts/UI/RankingRowUI.cs`

게임 HUD 표시 수정:

- `Assets/Scripts/UI/GameHUD.cs`

로그인/회원가입 수정:

- `Assets/Scripts/Backend/BackendManager.cs`
- `Assets/Scripts/Backend/BackendLoginUI.cs`

## 17. 현재 점검할 만한 부분

현재 기능은 빌드 가능한 상태지만, 유지보수 관점에서 아래 항목은 나중에 정리하면 좋습니다.

- `OnlineMatchManager.cs` 일부 사용자 표시 문자열 인코딩 점검 필요
- `OmokAI.HasNeighbor()`는 현재 사용되지 않음
- `RotatingCamera._tiltAngle` 필드는 할당되지만 사용되지 않아 빌드 경고 발생
- 온라인 결과 기록은 클라이언트 흐름에 의존하므로 예외 상황에서 중복/누락이 없는지 실제 기기 테스트 필요
- `RankingPanelUI._leaderboardUuid`는 Inspector에 직접 넣어야 하며, 비어 있으면 리더보드 UI가 표시되지 않음
- `MyRankRow`는 현재 순위와 점수만 표시하도록 스크립트가 맞춰져 있음

## 18. 신입 개발자 추천 학습 순서

1. `GameState.cs`, `GameManager.cs`로 전체 상태 흐름 파악
2. `BoardManager.cs`, `TurnManager.cs`로 보드와 턴 규칙 파악
3. `InputHandler.cs`, `StoneController.cs`로 클릭이 착수로 이어지는 과정 파악
4. `WinChecker.cs`, `RenjuRule.cs`로 오목/렌주 규칙 파악
5. `IGameMode.cs`, `SinglePlayMode.cs`, `AIPlayMode.cs`, `MultiPlayMode.cs`로 모드별 차이 파악
6. `OmokAI.cs`, `BoardEvaluator.cs`로 AI 판단 구조 파악
7. `BackendManager.cs`, `OnlineMatchManager.cs`로 뒤끝 연동 파악
8. `ResultUI.cs`, `RankingPanelUI.cs`, `GameHUD.cs`로 UI 흐름 파악

## 19. 간단 용어 정리

- 착수: 보드에 돌을 놓는 행위
- 금수: 흑돌이 둘 수 없는 자리
- 장목: 6목 이상
- 3-3: 열린 3이 동시에 두 개 생기는 수
- 4-4: 4가 동시에 두 개 생기는 수
- Min-Max: AI와 상대가 각각 최선의 선택을 한다고 가정해 수를 고르는 탐색 알고리즘
- Alpha-Beta: Min-Max 탐색에서 불필요한 분기를 줄이는 최적화
- 전이 테이블: 이미 평가한 보드 상태를 저장해 재사용하는 캐시
- Zobrist Hash: 보드 상태를 빠르게 해시 값으로 바꾸는 기법
- MatchEnd: 뒤끝 매치 결과를 서버에 제출하는 호출

