# 뒤끝 로그인 및 온라인 매칭 오목 가이드

이 프로젝트에는 뒤끝 Base SDK 로그인/회원가입과 뒤끝 Match 기반 1:1 온라인 오목 흐름이 추가되어 있습니다.

## 구현 파일

- `Assets/Scripts/Backend/BackendManager.cs`
  - 뒤끝 초기화
  - 아이디/비밀번호 커스텀 로그인
  - 아이디/비밀번호/닉네임 커스텀 회원가입
  - 회원가입 시 닉네임 생성
  - 로그인 시 기존 뒤끝 닉네임 조회
  - `Backend.Match.Poll()` 주기 호출

- `Assets/Scripts/Backend/BackendLoginUI.cs`
  - `LoginScene`의 입력 필드/버튼을 자동 탐색해 로그인/회원가입 연결
  - 로그인 성공 시 `GameScene` 로드

- `Assets/Scripts/Backend/BackendRuntimeBootstrap.cs`
  - 씬에 백엔드 매니저가 없어도 런타임에 자동 생성
  - `LoginScene`에서 로그인 UI 브릿지 자동 생성

- `Assets/Scripts/Backend/OnlineMatchManager.cs`
  - 매칭 서버 접속
  - 대기방 생성
  - 1:1 매칭 신청
  - 인게임 서버/게임방 접속
  - 바이너리 패킷으로 착수 좌표 송수신
  - 게임 종료 결과 전송

- `Assets/Scripts/Modes/MultiPlayMode.cs`
  - 기존 placeholder 멀티 모드를 뒤끝 온라인 대국 모드로 변경

## 실행 흐름

```text
LoginScene
  -> Backend.Initialize()
  -> 로그인: CustomLogin
  -> 회원가입: CustomSignUp -> UpdateNickname
  -> GameScene 로드

GameScene
  -> MultiBtn 클릭
  -> JoinMatchMakingServer
  -> CreateMatchRoom
  -> RequestMatchMaking
  -> JoinGameServer
  -> JoinGameRoom
  -> OnMatchInGameStart
  -> GameManager.StartGame(GameMode.Multi)
```

온라인 대국에서 착수는 다음 흐름으로 동기화됩니다.

```text
내 턴에 보드 클릭
  -> GameManager.OnBoardTapped()
  -> BoardManager.TryPlace()
  -> MultiPlayMode.OnStonePlace()
  -> Backend.Match.SendDataToInGameRoom()
  -> OnMatchRelay()
  -> 상대 클라이언트에서 GameManager.OnBoardTapped()
```

뒤끝 매치는 자신이 보낸 메시지도 다시 브로드캐스트합니다. 이 프로젝트는 이미 둔 위치에 대한 자기 자신의 echo 패킷은 `BoardManager.TryPlace()`에서 자연스럽게 무시되도록 처리합니다.

## 뒤끝 콘솔 설정

뒤끝 Match를 사용하려면 뒤끝 콘솔에서 1:1 매칭 카드를 생성해야 합니다.

권장 설정:

- 매치 모드: `OneOnOne`
- 매치 타입: 기본값은 코드에서 `MatchType.Random`
- 인원: 2명
- 결과 처리: 기본 또는 슈퍼 게이머 중 프로젝트 정책에 맞게 선택

`OnlineMatchManager`의 `_matchCardInDate`가 비어 있으면 `Backend.Match.GetMatchList()`로 첫 번째 `OneOnOne` 매칭 카드를 찾아 사용합니다. 특정 매칭 카드만 사용하고 싶으면 Unity Inspector에서 `OnlineMatchManager._matchCardInDate`에 콘솔의 inDate를 입력하세요.

## 주의사항

- 뒤끝 매치는 로그인과 닉네임이 필수입니다.
- 로그인 화면에서는 닉네임을 입력하지 않습니다. 닉네임은 회원가입 때 설정하고, 이후 로그인에서는 뒤끝에 저장된 기존 닉네임을 사용합니다.
- 공식 문서 기준으로 매치 이벤트는 `Backend.Match.Poll()`이 주기적으로 호출되어야 발생합니다.
- 온라인 대국에서는 무르기를 막아 두었습니다.
- 로컬에서 `GameScene`만 직접 실행하면 로그인 상태가 없으므로 Multi 버튼은 로그인 안내 토스트를 띄웁니다.
- `LoginScene`의 오브젝트 이름이 크게 바뀌면 `BackendLoginUI` 자동 바인딩이 실패할 수 있습니다. 이 경우 Inspector에서 입력 필드와 버튼을 직접 연결하세요.

## 참고한 공식 문서

- 뒤끝 Base 시작하기: https://docs.backnd.com/sdk-docs/backend/base/start-up/
- 뒤끝 커스텀 로그인: https://docs.backnd.com/sdk-docs/backend/base/user/custom/login/
- 뒤끝 Match 시작하기: https://docs.backnd.com/sdk-docs/backend/match/start-up/
- 뒤끝 Match 구조 및 Poll: https://docs.backnd.com/sdk-docs/backend/match/architecture/
- 매칭 신청: https://docs.backnd.com/sdk-docs/backend/match/server/request-match/request-match/
- 인게임 데이터 송수신: https://docs.backnd.com/sdk-docs/backend/match/ingame-server/send-data/send-data-to-game-room/
