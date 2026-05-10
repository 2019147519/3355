# 오목 온라인 점수 리더보드 가이드

온라인 대전 결과를 뒤끝 게임 정보 테이블에 저장하고, 리더보드는 `score` 기준으로 집계합니다.

점수 규칙:

- 승리: `score + 1`
- 패배: `score - 1`
- 최소 점수: `0`
- 무승부: 점수 변화 없음

승리 횟수(`wins`)와 패배 횟수(`losses`)도 함께 기록하지만, 순위 산정에는 `score`만 사용합니다.

## 뒤끝 콘솔 설정

### 1. 게임 정보 테이블

뒤끝 콘솔에서 게임 정보 테이블을 생성합니다.

- 테이블명: `omok_rank`
- 권한: `Private`
- 스키마 사용: 권장
- 컬럼:
  - `score`: Number, 기본값 `0`
  - `wins`: Number, 기본값 `0`
  - `losses`: Number, 기본값 `0`
  - `nickname`: String

주의:
- 리더보드에 연결할 테이블은 Public으로 변경하지 마세요. 뒤끝 리더보드 갱신 문서 기준으로 등록 테이블이 Public이면 갱신 오류가 날 수 있습니다.

### 2. 유저 리더보드

뒤끝 콘솔에서 유저 리더보드를 생성합니다.

- 리더보드 제목: `OmokWins`
- 대상: 유저
- 테이블: `omok_rank`
- 점수 컬럼: `score`
- 정렬: 내림차순
- 초기화 주기:
  - 누적 랭킹: `없음`
  - 시즌/주간 랭킹: 원하는 주기 선택
- 추가 항목:
  - `nickname` 권장
  - `wins`, `losses`도 함께 보여주고 싶다면 추가 항목으로 선택

기존에 `wins` 컬럼 기준으로 만든 리더보드가 있다면 새로 만들거나, 콘솔에서 점수 컬럼을 `score`로 맞춰주세요.

## Unity 구현

파일:

- `Assets/Scripts/Backend/BackendWinRankingManager.cs`
- `Assets/Scripts/UI/ResultUI.cs`

동작:

- 온라인 대전에서 승리하면 `score + 1`, `wins + 1`
- 온라인 대전에서 패배하면 `score - 1`, `losses + 1`
- `score`는 `0` 아래로 내려가지 않음
- 무승부는 변화 없음
- 리더보드 점수는 `score` 컬럼만 사용

## 참고 문서

- 유저 관리: https://docs.backnd.com/guide/getting-started/how-to-use/user/
- 게임 정보 관리: https://docs.backnd.com/guide/getting-started/how-to-use/game-information/
- 리더보드 개요: https://docs.backnd.com/guide/getting-started/how-to-use/rank/
- 유저 리더보드 갱신: https://docs.backnd.com/sdk-docs/backend/base/leaderboard/user/update/
