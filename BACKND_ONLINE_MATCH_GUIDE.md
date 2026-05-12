# 뒤끝 로그인 및 온라인 매치 가이드

최종 갱신일: 2026-05-12

이 문서는 현재 프로젝트의 뒤끝 Base SDK 로그인/회원가입과 뒤끝 Match SDK 기반 1:1 온라인 오목 구현 상태를 정리합니다.

## 1. 구현 파일

- `Assets/Scripts/Backend/BackendManager.cs`
  - 뒤끝 초기화
  - 커스텀 로그인/회원가입
  - 닉네임 조회/설정
  - `Backend.Match.Poll()` 주기 호출
  - `OnlineMatchManager`, `BackendWinRankingManager` 자동 생성

- `Assets/Scripts/Backend/BackendLoginUI.cs`
  - LoginScene의 로그인/회원가입 UI 연결
  - 로그인 성공 시 GameScene 로드
  - 로그인 화면에는 닉네임 입력 없음
  - 회원가입 화면에서만 닉네임 입력 사용

- `Assets/Scripts/Backend/BackendRuntimeBootstrap.cs`
  - 씬에 뒤끝 매니저가 없을 때 자동 생성

- `Assets/Scripts/Backend/OnlineMatchManager.cs`
  - 매치 서버 접속
  - 대기방 생성
  - 1:1 랜덤 매칭 요청
  - 게임 서버/게임 방 접속
  - 착수 패킷 송수신
  - 흑돌/백돌 랜덤 결정
  - 재대결 동기화
  - 매치 결과 제출
  - 매칭 취소/세션 정리

- `Assets/Scripts/Modes/MultiPlayMode.cs`
  - 온라인 대국 중 내 턴에만 입력 허용
  - 내 착수만 서버로 전송

- `Assets/Scripts/UI/OnlineMatchStatusUI.cs`
  - 매칭 진행 상태 패널 표시
  - 취소 버튼 처리

- `Assets/Scripts/UI/ResultUI.cs`
  - 온라인 결과 보관
  - 승패에 따른 랭킹 갱신
  - 온라인 재대결/나가기 처리

## 2. 로그인 흐름

```text
LoginScene
  -> BackendLoginUI.Awake()
  -> BackendManager 생성
  -> Backend.Initialize()
  -> 로그인 버튼
  -> Backend.BMember.CustomLogin(id, password)
  -> 저장된 닉네임 조회
  -> CurrentNickname 저장
  -> GameScene 로드
```

회원가입 흐름:

```text
회원가입 버튼
  -> Backend.BMember.CustomSignUp(id, password)
  -> Backend.BMember.UpdateNickname(nickname)
  -> CurrentNickname 저장
  -> GameScene 로드
```

닉네임 규칙:

- 로그인 시에는 닉네임 input이 없습니다.
- 회원가입 시에만 닉네임을 입력합니다.
- 로그인 후 닉네임은 뒤끝 저장값을 우선 사용합니다.
- 닉네임이 비어 있으면 아이디를 fallback으로 사용합니다.
- 마지막 닉네임은 `PlayerPrefs`의 `BackendLastNickname`에 저장됩니다.

## 3. 온라인 매칭 흐름

```text
GameScene
  -> Multi 버튼
  -> OnlineMatchManager.StartQuickMatch()
  -> 이전 연결 정리
  -> Backend.Match.JoinMatchMakingServer()
  -> Backend.Match.CreateMatchRoom()
  -> Backend.Match.RequestMatchMaking()
  -> Backend.Match.JoinGameServer()
  -> Backend.Match.JoinGameRoom()
  -> OnMatchInGameStart()
  -> GameManager.StartGame(GameMode.Multi)
```

매칭 중 UI:

- `OnlineMatchStatusUI.Show(message, canCancel)`로 상태 패널을 표시합니다.
- 취소 버튼은 텍스트가 없는 이미지 버튼 기준입니다.
- 취소 가능 상태에서는 `OnlineMatchManager.CancelMatchmaking()`을 호출합니다.
- 실패/완료 후 닫기만 필요한 상태에서는 버튼 클릭 시 패널만 닫습니다.

## 4. 착수 동기화

내 착수:

```text
GameManager.OnBoardTapped()
  -> BoardManager.TryPlace()
  -> MultiPlayMode.OnStonePlace()
  -> OnlineMatchManager.SendMove()
  -> Backend.Match.SendDataToInGameRoom()
```

상대 착수:

```text
OnlineMatchManager.OnMatchRelay()
  -> 패킷 파싱
  -> IsApplyingRemoteMove = true
  -> GameManager.OnBoardTapped(row, col)
  -> IsApplyingRemoteMove = false
```

`IsApplyingRemoteMove`는 상대 착수를 다시 서버로 보내는 echo 문제를 막기 위한 플래그입니다.

## 5. 흑돌/백돌 결정

온라인 매치에서는 흑돌/백돌을 고정 닉네임 기준으로 정하지 않습니다.

초기 대국:

- 방 토큰과 참가자 닉네임 목록으로 seed 생성
- 닉네임을 정렬한 뒤 seed 값으로 흑돌 인덱스 결정
- 양쪽 클라이언트가 같은 seed와 닉네임 목록을 사용하므로 같은 결과를 계산

재대결:

- 양쪽 클라이언트가 재대결 수락 패킷에 seed를 담아 전송
- 두 seed를 XOR해서 새 `CurrentColorSeed` 생성
- 새 seed로 흑돌/백돌을 다시 결정

관련 값:

- `OnlineMatchManager.BlackNickname`
- `OnlineMatchManager.WhiteNickname`
- `OnlineMatchManager.LocalPlayer`
- `OnlineMatchManager.CurrentColorSeed`

## 6. 재대결 흐름

게임 종료 후 `ResultUI`가 재대결 화면을 표시합니다.

O를 누른 경우:

- `OnlineMatchManager.SendRematchChoice(true)`
- 상대 응답을 기다림
- 15초 타임아웃 시작

X를 누른 경우:

- `OnlineMatchManager.SendRematchChoice(false)`
- `OnlineMatchManager.FinishOnlineSession()`
- PostGameView로 이동

상대가 나가거나 X를 선택한 경우:

- `OnOpponentDeclinedRematch` 이벤트 발생
- 내가 O를 눌러 기다리던 중이면 상대가 나갔다는 메시지 표시
- 온라인 세션 정리
- PostGameView로 이동

양쪽 모두 O인 경우:

- 새 seed로 흑돌/백돌 다시 결정
- `OnRematchAccepted` 이벤트 발생
- `GameManager.StartGame(GameMode.Multi)`로 재시작

## 7. 매치 결과 제출

온라인 게임이 끝나면 `ResultUI.Show()`에서 `OnlineMatchManager.SetPendingResult(winner)`로 결과를 보관합니다.

세션을 끝낼 때 `FinishOnlineSession()`이 호출되면:

```text
SubmitPendingResult()
  -> SubmitResult(winner)
  -> MatchGameResult 생성
  -> Backend.Match.MatchEnd(result)
```

결과 제출에는 흑돌/백돌 닉네임과 각 사용자의 `SessionId`가 필요합니다.

- 흑 승리: 흑 세션을 winners, 백 세션을 losers
- 백 승리: 백 세션을 winners, 흑 세션을 losers
- 무승부: 둘 다 draws

## 8. 뒤끝 콘솔 설정

뒤끝 콘솔에서 1:1 매치 카드를 생성해야 합니다.

권장 설정:

- 매치 모드: `OneOnOne`
- 매치 타입: 코드 기준 `MatchType.Random`
- 인원: 2명
- 결과 처리: 프로젝트 정책에 맞게 설정

`OnlineMatchManager._matchCardInDate`:

- 특정 매치 카드만 사용할 경우 Inspector에 inDate 입력
- 비워두면 `Backend.Match.GetMatchList()`로 첫 번째 OneOnOne 매치 카드를 찾아 사용

## 9. 주의 사항

- 뒤끝 Match 이벤트를 받으려면 `Backend.Match.Poll()`이 계속 호출되어야 합니다.
- 온라인 대전에서는 무르기를 막고 있습니다.
- 게임이 끝난 뒤 연결 정리를 하지 않으면 다음 매칭에서 `TCP Client is working` 오류가 날 수 있습니다.
- 연결이 끊긴 상태에서 패킷을 보내면 `Not connected` 오류가 날 수 있으므로 `IsInGameRoom` 확인이 필요합니다.
- `OnlineMatchManager.cs`의 일부 사용자 표시 문자열은 인코딩 점검이 필요합니다.

## 10. 참고 문서

- 뒤끝 Base 시작하기: https://docs.backnd.com/sdk-docs/backend/base/start-up/
- 뒤끝 커스텀 로그인: https://docs.backnd.com/sdk-docs/backend/base/user/custom/login/
- 뒤끝 Match 시작하기: https://docs.backnd.com/sdk-docs/backend/match/start-up/
- 뒤끝 Match 구조와 Poll: https://docs.backnd.com/sdk-docs/backend/match/architecture/
- 매칭 요청: https://docs.backnd.com/sdk-docs/backend/match/server/request-match/request-match/
- 게임 방 데이터 송수신: https://docs.backnd.com/sdk-docs/backend/match/ingame-server/send-data/send-data-to-game-room/

