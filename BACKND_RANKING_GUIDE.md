# 오목 온라인 점수 리더보드 가이드

최종 갱신일: 2026-05-12

온라인 대전 결과를 뒤끝 게임 정보 테이블에 저장하고, 유저 리더보드에서 `score` 기준으로 순위를 집계합니다.

## 1. 점수 규칙

- 승리: `score + 1`, `wins + 1`
- 패배: `score - 1`, `losses + 1`
- 최소 점수: `0`
- 무승부: 점수 변화 없음

승리 수(`wins`)와 패배 수(`losses`)는 전적 기록용입니다. 순위 산정에는 `score`만 사용합니다.

## 2. 뒤끝 콘솔 설정

### 게임 정보 테이블

뒤끝 콘솔에서 게임 정보 테이블을 생성합니다.

- 테이블명: `omok_rank`
- 권한: `Private`
- 스키마 사용 권장

컬럼:

| 컬럼명 | 타입 | 기본값 | 용도 |
| --- | --- | --- | --- |
| `score` | Number | 0 | 랭킹 기준 점수 |
| `wins` | Number | 0 | 온라인 승리 수 |
| `losses` | Number | 0 | 온라인 패배 수 |
| `nickname` | String | 빈 값 | 표시용 닉네임 |

주의:

- 리더보드 갱신용 테이블은 Public으로 바꾸지 않는 것을 권장합니다.
- 기존에 `wins` 기준 리더보드를 만들었다면 새로 만들거나 점수 컬럼을 `score`로 맞춰야 합니다.

### 유저 리더보드

뒤끝 콘솔에서 유저 리더보드를 생성합니다.

- 리더보드 제목: `OmokWins`
- 대상 테이블: `omok_rank`
- 점수 컬럼: `score`
- 정렬: 내림차순
- 추가 표시 항목: `nickname` 권장, 필요하면 `wins`, `losses` 추가

프로젝트 코드의 기본 리더보드 제목은 `OmokWins`입니다.

## 3. Unity 구현 파일

- `Assets/Scripts/Backend/BackendWinRankingManager.cs`
  - 랭킹 row 생성/조회
  - 승리/패배에 따른 점수 갱신
  - 리더보드 uuid 조회
  - `UpdateMyDataAndRefreshLeaderboard()` 호출

- `Assets/Scripts/UI/ResultUI.cs`
  - 온라인 게임 종료 시 승패 판정
  - 내 승리면 `ReportOnlineWin()`
  - 내 패배면 `ReportOnlineLoss()`

- `Assets/Scripts/UI/RankingPanelUI.cs`
  - 리더보드 목록 조회
  - row prefab 생성
  - 내 순위 row 갱신
  - 닫기 버튼 처리

- `Assets/Scripts/UI/RankingRowUI.cs`
  - 일반 row: 순위, 닉네임, 점수 표시
  - MyRankRow: 순위와 점수만 표시 가능

## 4. 데이터 갱신 흐름

```text
온라인 게임 종료
  -> ResultUI.Show(winner)
  -> winner == OnlineMatchManager.LocalPlayer
       -> BackendWinRankingManager.ReportOnlineWin()
     else
       -> BackendWinRankingManager.ReportOnlineLoss()
  -> EnsureLeaderboardUuid()
  -> EnsureRankingRow()
  -> score/wins/losses 계산
  -> Backend.Leaderboard.User.UpdateMyDataAndRefreshLeaderboard()
```

패배 시 점수 계산:

```text
nextScore = Mathf.Max(0, currentScore - 1)
```

따라서 점수는 0점 아래로 내려가지 않습니다.

## 5. 리더보드 UI 세팅

`RankingPanelUI` Inspector 연결:

- `_content`: ScrollView의 Content Transform
- `_rowPrefab`: 순위, 닉네임, 점수를 가진 row prefab
- `_myRankRow`: 하단 고정 내 순위 row
- `_closeButton`: 리더보드 창 닫기 버튼
- `_leaderboardUuid`: 뒤끝 콘솔에서 생성한 리더보드 uuid
- `_limit`: 표시할 최대 row 수

`RankingRowUI` Inspector 연결:

- 일반 row prefab
  - `_rankText`
  - `_nicknameText`
  - `_scoreText`

- MyRankRow
  - `_rankText`
  - `_scoreText`
  - `_nicknameText`는 없어도 됨

현재 MyRankRow는 자신의 닉네임 표시를 제거한 UI 기준으로 동작합니다.

## 6. 리더보드가 안 뜰 때 확인할 것

- `RankingPanelUI._leaderboardUuid`가 비어 있지 않은지
- 뒤끝 콘솔의 리더보드 점수 컬럼이 `score`인지
- `omok_rank` 테이블이 존재하는지
- 로그인 상태인지
- 온라인 대전 결과가 한 번 이상 기록되었는지
- `_content`와 `_rowPrefab`이 Inspector에 연결되어 있는지
- row prefab에 `RankingRowUI`가 붙어 있는지
- ScrollView Content에 `VerticalLayoutGroup`, `ContentSizeFitter`가 정상 적용되는지

## 7. 참고 문서

- 유저 관리: https://docs.backnd.com/guide/getting-started/how-to-use/user/
- 게임 정보 관리: https://docs.backnd.com/guide/getting-started/how-to-use/game-information/
- 리더보드 개요: https://docs.backnd.com/guide/getting-started/how-to-use/rank/
- 유저 리더보드 갱신: https://docs.backnd.com/sdk-docs/backend/base/leaderboard/user/update/

