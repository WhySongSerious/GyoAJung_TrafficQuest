# Traffic Quest

Logitech G29 휠·페달로 조작하는 Unity 운전 시뮬레이터입니다.
아주대학교 파란학기 팀 프로젝트로 진행했습니다.

| 항목 | 내용 |
| --- | --- |
| 기간 | 2024.09 – 2024.12 |
| 형태 | 팀 프로젝트 (아주대학교 파란학기) |
| 엔진 | Unity 2022.3.50f1 · C# |
| 입력 장치 | Logitech G29 휠·페달 (Logitech Gaming SDK) |

## 내가 맡은 일 — 송재혁

- **차량 주행 (팀원과 공동 작업):** G29 휠·페달 입력을 정규화해 WheelCollider 기반 차량 제어에 연결하고, 방향지시등 등 차량 조명을 구현했습니다. 브레이크를 밟는 중에도 저속 자동 가속이 걸리던 문제를 수정했습니다.
- **UI·씬 흐름:** 메인 메뉴 → 게임 선택 → 로딩 → 주행 스테이지로 이어지는 화면 흐름을 만들었습니다.
- **차량 사운드:** 페달 입력에 따라 주행음과 공회전음을 전환하고, 가속 정도에 맞춰 볼륨이 부드럽게 바뀌게 했습니다. 방향지시등 소리는 Logitech SDK로 G29 버튼 입력을 읽어 좌·우 상태를 전환하고, 짧은 시간의 중복 입력은 무시합니다.
- **저장소 운영:** 팀 저장소를 개설하고 팀원별 브랜치 작업을 모았습니다.

> 함께 작업한 내용을 한 사람의 계정으로 커밋한 경우가 있어, 커밋 작성자와 실제 작업자가 일치하지 않는 부분이 있습니다.

## 코드 위치

| 내용 | 경로 |
| --- | --- |
| 휠·페달 입력 | [`Assets/Script/LogitechInput.cs`](Assets/Script/LogitechInput.cs), [`SDKInputManager.cs`](Assets/Script/SDKInputManager.cs) |
| 차량 제어·조명 | [`Assets/Script/Player.cs`](Assets/Script/Player.cs) |
| 메인 메뉴·게임 선택 | [`Assets/재혁 UIUX/`](Assets/재혁%20UIUX) |
| 차량 사운드·로딩 화면 | [`JaeHyuk` 브랜치](../../tree/JaeHyuk) — `Assets/Sound/CarSound.cs`, `Assets/Sound/TwinkleCar.cs` |

## 실행

1. Unity 2022.3.50f1로 프로젝트를 엽니다.
2. `Assets/재혁 UIUX/SCENES/MainMenu.unity`에서 시작합니다. (빌드 순서: MainMenu → GameSelect → SampleScene)
3. 주행 조작에는 Logitech G29와 Logitech Gaming SDK(G HUB) 연결이 필요합니다.

## 출처

지형·도로·차량 등 Asset Store 에셋은 각 에셋의 라이선스를 따릅니다.
