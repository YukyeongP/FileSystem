# FileSystem
- 물리이미지: 저장장치 전체 이미지 뜬 것,
- 로지컬이미지: 분석 끝난 결과 이미지, 파일 디렉토리 구조 보임

<FileSystem 용어>
- Partition Table: 물리이미지 뜨면 제일 처음 나오는 것
- slack: 물리적, 논리적 구조 차이로 낭비되는 공간, 유의미한 데이터 있을 수 있음
- Partition 구조
1) Meta data
- Super Block(geometry of fs): 블록크기(sector size x sector count per cluster 
				  => cluster size), 
	      루트 iNode 위치, 저널
- Bitmap: 할당, 활성화 된 블록인지 아닌지 정보 알려줌, unused area
- iNode Table: 파일 or 디렉토리 / 데이터위치정보, 고정크기
2) Real data
- Data Block: 실제 데이터(파일 데이터+엔트리정보+저널정보+비할당)

<하드디스크와 플래시메모리 차이>
- 플래시메모리는 특정 위치에 가서 수정 못하고 전체를 다 바꿔야함.
- 하드디스크는 한 글자만 변경 가능
