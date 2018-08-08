using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState{
    wait,
    move,
    win,
    lose,
    pause
}

public enum TileKind
{
	Breakable,
    Blank,
    Lock,
    Concrete,
    Slime,
    Normal
}

[System.Serializable]
public class MatchType
{
    public int type;
    public string color;
}

[System.Serializable]
public class TileType{
	public int x;
	public int y;
	public TileKind tileKind;
}

public class Board : MonoBehaviour {

    [Header ("Scriptable Object Stuff")]
    public World world;
    public int level;

    public GameState currentState = GameState.move;
    [Header ("Board Dimensions")]
    public int width;
    public int height;
    public int offSet;

    [Header("Prefabs")]
    public GameObject tilePrefab;
	public GameObject breakableTilePrefab;
    public GameObject lockTilePrefab;
    public GameObject concreteTilePrefab;
    public GameObject[] dots;
    public GameObject destroyParticle;

    [Header ("Layout")]
	public TileType[] boardLayout;
    private bool[,] blankSpaces;
	private BackgroundTile[,] breakableTiles;
    public BackgroundTile[,] lockTiles;
    private BackgroundTile[,] concreteTiles;
    public GameObject[,] allDots;

    [Header("Match Stuff")]
    public MatchType matchType;
    public Dot currentDot;
	private FindMatches findMatches;
	public int basePieceValue = 20;
	private int streakValue = 1;
	private ScoreManager scoreManager;
	private SoundManager soundManager;
    private GoalManager goalManager;
	public float refillDelay = 0.5f;
	public int[] scoreGoals;


    private void Awake()
    {
        if(PlayerPrefs.HasKey("Current Level"))
        {
            level = PlayerPrefs.GetInt("Current Level");
        }
        if(world != null)
        {
            if (level < world.levels.Length)
            {
                if (world.levels[level] != null)
                {
                    width = world.levels[level].width;
                    height = world.levels[level].height;
                    dots = world.levels[level].dots;
                    scoreGoals = world.levels[level].scoreGoals;
                    boardLayout = world.levels[level].boardLayout;
                }
            }
        }
    }

    // Use this for initialization
    void Start () {
        goalManager = FindObjectOfType<GoalManager>();
		soundManager = FindObjectOfType<SoundManager>();
		scoreManager = FindObjectOfType<ScoreManager>();
		breakableTiles = new BackgroundTile[width, height];
        lockTiles = new BackgroundTile[width, height];
        concreteTiles = new BackgroundTile[width, height];
        findMatches = FindObjectOfType<FindMatches>();
		blankSpaces = new bool[width, height];
        allDots = new GameObject[width, height];
        SetUp();
        currentState = GameState.pause;
	}

	public void GenerateBlankSpaces(){
		for (int i = 0; i < boardLayout.Length; i ++)
		{
			if(boardLayout[i].tileKind == TileKind.Blank)
			{
				blankSpaces[boardLayout[i].x, boardLayout[i].y] = true;
			}
		}
	}

    public void GenerateBreakableTiles()
	{
		//Look at all the tiles in the layout
		for (int i = 0; i < boardLayout.Length; i ++)
		{
			//if a tile is a "Jelly" tile
			if(boardLayout[i].tileKind == TileKind.Breakable)
			{
				//Create a "Jelly" tile at that position;
				Vector2 tempPosition = new Vector2(boardLayout[i].x, boardLayout[i].y);
				GameObject tile = Instantiate(breakableTilePrefab,tempPosition, Quaternion.identity);
				breakableTiles[boardLayout[i].x, boardLayout[i].y] = tile.GetComponent<BackgroundTile>();
			}
		}
	}

    private void GenerateLockTiles()
    {
        //Look at all the tiles in the layout
        for (int i = 0; i < boardLayout.Length; i++)
        {
            //if a tile is a "Lock" tile
            if (boardLayout[i].tileKind == TileKind.Lock)
            {
                //Create a "Lock" tile at that position;
                Vector2 tempPosition = new Vector2(boardLayout[i].x, boardLayout[i].y);
                GameObject tile = Instantiate(lockTilePrefab, tempPosition, Quaternion.identity);
                lockTiles[boardLayout[i].x, boardLayout[i].y] = tile.GetComponent<BackgroundTile>();
            }
        }
    }

    private void GenerateConcreteTiles()
    {
        //Look at all the tiles in the layout
        for (int i = 0; i < boardLayout.Length; i++)
        {
            //if a tile is a "Lock" tile
            if (boardLayout[i].tileKind == TileKind.Concrete)
            {
                //Create a "Lock" tile at that position;
                Vector2 tempPosition = new Vector2(boardLayout[i].x, boardLayout[i].y);
                GameObject tile = Instantiate(concreteTilePrefab, tempPosition, Quaternion.identity);
                concreteTiles[boardLayout[i].x, boardLayout[i].y] = tile.GetComponent<BackgroundTile>();
            }
        }
    }

    private void SetUp(){
		GenerateBlankSpaces();
		GenerateBreakableTiles();
        GenerateLockTiles();
        GenerateConcreteTiles();
        for (int i = 0; i < width; i ++){
			for (int j = 0; j < height; j++)
			{
                if (!blankSpaces[i, j] && !concreteTiles[i,j])
				{
					Vector2 tempPosition = new Vector2(i, j + offSet);
					Vector2 tilePosition = new Vector2(i, j);
					GameObject backgroundTile = Instantiate(tilePrefab, tilePosition, Quaternion.identity) as GameObject;
					backgroundTile.transform.parent = this.transform;
					backgroundTile.name = "( " + i + ", " + j + " )";

					int dotToUse = Random.Range(0, dots.Length);

					int maxIterations = 0;

					while (MatchesAt(i, j, dots[dotToUse]) && maxIterations < 100)
					{
						dotToUse = Random.Range(0, dots.Length);
						maxIterations++;
					}
					maxIterations = 0;

					GameObject dot = Instantiate(dots[dotToUse], tempPosition, Quaternion.identity);
                    dot.GetComponent<Dot>().row = j;
					dot.GetComponent<Dot>().column = i;
					dot.transform.parent = this.transform;
					dot.name = "( " + i + ", " + j + " )";
					allDots[i, j] = dot;
				}
			}

        }
    }

    private bool MatchesAt(int column, int row, GameObject piece){
        if(column > 1 && row > 1){
			if (allDots[column - 1, row] != null && allDots[column - 2, row] != null)
			{
				if (allDots[column - 1, row].tag == piece.tag && allDots[column - 2, row].tag == piece.tag)
				{
					return true;
				}
			}
			if (allDots[column, row - 1] != null && allDots[column, row - 2] != null)
			{
				if (allDots[column, row - 1].tag == piece.tag && allDots[column, row - 2].tag == piece.tag)
				{
					return true;
				}
			}

        }else if(column <= 1 || row <= 1){
            if(row > 1){
				if (allDots[column, row - 1] != null && allDots[column, row - 2] != null)
				{
					if (allDots[column, row - 1].tag == piece.tag && allDots[column, row - 2].tag == piece.tag)
					{
						return true;
					}
				}
            }
            if (column > 1)
            {
				if (allDots[column - 1, row] != null && allDots[column - 2, row] != null)
				{
					if (allDots[column - 1, row].tag == piece.tag && allDots[column - 2, row].tag == piece.tag)
					{
						return true;
					}
				}
            }
        }

        return false;
    }

    private MatchType ColumnOrRow(){
        //Make a copy of the current matches
        List<GameObject> matchCopy = findMatches.currentMatches as List<GameObject>;

        matchType.type = 0;
        matchType.color = "";

        //Cycle through all of match Copy and decide if a bomb needs to be made
        for (int i = 0; i < matchCopy.Count; i ++)
        {
            //Store this dot
            Dot thisDot = matchCopy[i].GetComponent<Dot>();
            string color = matchCopy[i].tag;
            int column = thisDot.column;
            int row = thisDot.row;
            int columnMatch = 0;
            int rowMatch = 0;
            //Cycle through the rest of the pieces and compare
            for (int j = 0; j < matchCopy.Count; j ++)
            {
                //Store the next dot
                Dot nextDot = matchCopy[j].GetComponent<Dot>();
                if(nextDot == thisDot){
                    continue;
                }
                if(nextDot.column == thisDot.column && nextDot.tag == color)
                {
                    columnMatch++;
                }
                if (nextDot.row == thisDot.row && nextDot.tag == color)
                {
                    rowMatch++;
                }
            }
            // Return 3 if column or row match
            //Return 2 if adjacent
            //Return 1 if it's a color bomb
            if(columnMatch == 4 || rowMatch == 4)
            {
                matchType.type = 1;
                matchType.color = color;
                return matchType;
            }
            else if(columnMatch == 2 && rowMatch == 2)
            {
                matchType.type = 2;
                matchType.color = color;
                return matchType;
            }
            else if(columnMatch == 3 || rowMatch == 3)
            {
                matchType.type = 3;
                matchType.color = color;
                return matchType;
            }
        }
        matchType.type = 0;
        matchType.color = "";
        return matchType;
    }

    private void CheckToMakeBombs(){
        //How many objects are in findMatches currentMatches?
        if (findMatches.currentMatches.Count > 3)
        {
            //What type of match?
            MatchType typeOfMatch = ColumnOrRow();
            if (typeOfMatch.type == 1)
            {
                //Make a color bomb
                //is the current dot matched?
                if (currentDot != null && currentDot.isMatched && currentDot.tag == typeOfMatch.color)
                {
                    currentDot.isMatched = false;
                    currentDot.MakeColorBomb();
                }
                else
                {
                    if (currentDot.otherDot != null)
                    {
                        Dot otherDot = currentDot.otherDot.GetComponent<Dot>();
                        if (otherDot.isMatched && otherDot.tag == typeOfMatch.color)
                        {
                            otherDot.isMatched = false;
                            otherDot.MakeColorBomb();
                        }
                    }
                }
            }

            else if (typeOfMatch.type == 2)
            {
                //Make a adjacent bomb
                //is the current dot matched?
                if (currentDot != null && currentDot.isMatched && currentDot.tag == typeOfMatch.color)
                {
                    currentDot.isMatched = false;
                    currentDot.MakeAdjacentBomb();
                }
                else if (currentDot.otherDot != null)
                {
                    Dot otherDot = currentDot.otherDot.GetComponent<Dot>();
                    if (otherDot.isMatched && otherDot.tag == typeOfMatch.color)
                    {
                        otherDot.isMatched = false;
                        otherDot.MakeAdjacentBomb();
                    }
                }
            }
            else if (typeOfMatch.type == 3)
            {
                findMatches.CheckBombs(typeOfMatch);
            }
        }
    }

    private void DestroyMatchesAt(int column, int row){
        if(allDots[column, row].GetComponent<Dot>().isMatched){
            //Does a tile need to break?
			if(breakableTiles[column, row]!=null)
			{
				//if it does, give one damage.
				breakableTiles[column, row].TakeDamage(1);
				if(breakableTiles[column, row].hitPoints <= 0)
				{
					breakableTiles[column, row] = null;
				}
			}
            if (lockTiles[column, row] != null)
            {
                //if it does, give one damage.
                lockTiles[column, row].TakeDamage(1);
                if (lockTiles[column, row].hitPoints <= 0)
                {
                    lockTiles[column, row] = null;
                }
            }
            DamageConcrete(column, row);
            if(goalManager != null)
            {
                goalManager.CompareGoal(allDots[column, row].tag.ToString());
                goalManager.UpdateGoals();
            }

            //Does the sound manager exist?
			if(soundManager != null)
			{
				soundManager.PlayRandomDestroyNoise();
			}
            GameObject particle = Instantiate(destroyParticle, 
                                              allDots[column, row].transform.position, 
                                              Quaternion.identity);
            Destroy(particle, .5f);
            //allDots[column, row].GetComponent<Dot>().PopAnimation();
            Destroy(allDots[column, row]);
			scoreManager.IncreaseScore(basePieceValue * streakValue);
            allDots[column, row] = null;
        }
    }

    public void DestroyMatches(){
        //How many elements are in the matched pieces list from findmatches?
        if (findMatches.currentMatches.Count >= 4)
        {
            CheckToMakeBombs();
        }
        findMatches.currentMatches.Clear();
        for (int i = 0; i < width; i ++){
            for (int j = 0; j < height; j++){
                if (allDots[i, j] != null){
                    DestroyMatchesAt(i, j);
                }
            }
        }
        StartCoroutine(DecreaseRowCo2());
    }

    private void DamageConcrete(int column, int row)
    {
        if(column > 0)
        {
            if(concreteTiles[column - 1, row])
            {
                concreteTiles[column - 1, row].TakeDamage(1);
                if (concreteTiles[column - 1, row].hitPoints <= 0)
                {
                    concreteTiles[column - 1, row] = null;
                }
            }
        }
        if (column < width - 1)
        {
            if (concreteTiles[column + 1, row])
            {
                concreteTiles[column + 1, row].TakeDamage(1);
                if (concreteTiles[column + 1, row].hitPoints <= 0)
                {
                    concreteTiles[column + 1, row] = null;
                }
            }
        }
        if (row > 0)
        {
            if (concreteTiles[column, row - 1])
            {
                concreteTiles[column, row - 1].TakeDamage(1);
                if (concreteTiles[column, row - 1].hitPoints <= 0)
                {
                    concreteTiles[column, row - 1] = null;
                }
            }
        }
        if (row < height -1)
        {
            if (concreteTiles[column, row + 1])
            {
                concreteTiles[column, row + 1].TakeDamage(1);
                if (concreteTiles[column, row + 1].hitPoints <= 0)
                {
                    concreteTiles[column, row + 1] = null;
                }
            }
        }
    }

	private IEnumerator DecreaseRowCo2()
	{
        //yield return new WaitForSeconds(.5f);
		for (int i = 0; i < width; i ++)
		{
			for (int j = 0; j < height; j ++)
			{
				//if the current spot isn't blank and is empty. . . 
                if(!blankSpaces[i,j] && allDots[i,j] == null)
				{
					//loop from the space above to the top of the column
					for (int k = j + 1; k < height; k ++)
					{
						//if a dot is found. . .
						if(allDots[i, k]!= null)
						{
							//move that dot to this empty space
							allDots[i, k].GetComponent<Dot>().row = j;
							//set that spot to be null
							allDots[i, k] = null;
							//break out of the loop;
							break;
						}
					}
				}
			}
		}
		yield return new WaitForSeconds(refillDelay * 0.5f);
        Debug.Log("Refilling the board");
		StartCoroutine(FillBoardCo());
	}

    private IEnumerator DecreaseRowCo(){
        int nullCount = 0;
        for (int i = 0; i < width; i ++){
            for (int j = 0; j < height; j ++){
                if(allDots[i, j] == null){
                    nullCount++;
                }else if(nullCount > 0){
                    allDots[i, j].GetComponent<Dot>().row -= nullCount;
                    allDots[i, j] = null;
                }
            }
            nullCount = 0;
        }
		yield return new WaitForSeconds(refillDelay * 0.5f);
        StartCoroutine(FillBoardCo());
    }

    private void RefillBoard(){
        for (int i = 0; i < width; i ++){
            for (int j = 0; j < height; j ++){
                if(allDots[i, j] == null && !blankSpaces[i,j]){
                    Vector2 tempPosition = new Vector2(i, j + offSet);
                    int dotToUse = Random.Range(0, dots.Length);
					int maxIterations = 0;

					while(MatchesAt(i, j, dots[dotToUse]) && maxIterations < 100)
					{
						maxIterations++;
						dotToUse = Random.Range(0, dots.Length);
					}

					maxIterations = 0;
					GameObject piece = Instantiate(dots[dotToUse], tempPosition, Quaternion.identity);
                    allDots[i, j] = piece;
                    piece.GetComponent<Dot>().row = j;
                    piece.GetComponent<Dot>().column = i;

                }
            }
        }
    }

    private bool MatchesOnBoard(){
        findMatches.FindAllMatches();
        for (int i = 0; i < width; i ++){
            for (int j = 0; j < height; j ++){
                if(allDots[i, j]!= null){
                    if(allDots[i, j].GetComponent<Dot>().isMatched){
                        return true;
                    }
                }
            }
        }
        return false;
    }

    private IEnumerator FillBoardCo(){
		yield return new WaitForSeconds(refillDelay);
        RefillBoard();
        yield return new WaitForSeconds(refillDelay);
        while(MatchesOnBoard()){
			streakValue ++;
			DestroyMatches();
            yield break;
        }
        currentDot = null;

		if(IsDeadlocked())
		{
			StartCoroutine(ShuffleBoard());
		}
		yield return new WaitForSeconds(refillDelay);
        Debug.Log("Done Refilling");
        System.GC.Collect();
        currentState = GameState.move;
		streakValue = 1;

    }

	private void SwitchPieces(int column, int row, Vector2 direction)
	{
		//Take the second piece and save it in a holder
		GameObject holder = allDots[column + (int)direction.x, row + (int)direction.y] as GameObject;
        //switching the first dot to be the second position
		allDots[column + (int)direction.x, row + (int)direction.y] = allDots[column, row];
		//Set the first dot to be the second dot
		allDots[column, row] = holder;
	}

    private bool CheckForMatches()
	{
		for (int i = 0; i < width; i ++)
		{
			for (int j = 0; j < height; j ++)
			{
				if(allDots[i,j]!= null)
				{
					//Make sure that one and two to the right are in the
					//board
					if (i < width - 2)
					{
						//Check if the dots to the right and two to the right exist
						if (allDots[i + 1, j] != null && allDots[i + 2, j] != null)
						{
							if (allDots[i + 1, j].tag == allDots[i, j].tag
							   && allDots[i + 2, j].tag == allDots[i, j].tag)
							{
								return true;
							}
						}

					}
					if (j < height - 2)
					{
						//Check if the dots above exist
						if (allDots[i, j + 1] != null && allDots[i, j + 2] != null)
						{
							if (allDots[i, j + 1].tag == allDots[i, j].tag
							   && allDots[i, j + 2].tag == allDots[i, j].tag)
							{
								return true;
							}
						}
					}
				}
			}
		}
		return false;
	}

	public bool SwitchAndCheck(int column, int row, Vector2 direction)
	{
		SwitchPieces(column, row, direction);
		if(CheckForMatches())
		{
			SwitchPieces(column, row, direction);
			return true;
		}
		SwitchPieces(column, row, direction);
		return false;
	}

    private bool IsDeadlocked()
	{
		for (int i = 0; i < width; i ++)
		{
			for (int j = 0; j < height; j ++)
			{
				if(allDots[i,j]!= null)
				{
					if(i < width - 1)
					{
						if(SwitchAndCheck(i, j, Vector2.right))
						{
							return false;
						}
					}
					if(j < height - 1)
					{
						if(SwitchAndCheck(i,j,Vector2.up))
						{
							return false;
						}
					}
				}
			}
		}
		return true;
	}

	private IEnumerator ShuffleBoard()
	{
		yield return new WaitForSeconds(0.5f);
		//Create a list of game objects
		List<GameObject> newBoard = new List<GameObject>();
		//Add every piece to this list
		for (int i = 0; i < width; i ++)
		{
			for (int j = 0; j < height; j ++)
			{
				if(allDots[i,j]!= null)
				{
					newBoard.Add(allDots[i, j]);
				}
			}
		}
		yield return new WaitForSeconds(0.5f);
		//for every spot on the board. . . 
		for (int i = 0; i < width; i ++)
		{
			for (int j = 0; j < height; j ++)
			{
				//if this spot shouldn't be blank
				if(!blankSpaces[i,j])
				{
					//Pick a random number
					int pieceToUse = Random.Range(0, newBoard.Count);
                    
                    //Assign the column to the piece
					int maxIterations = 0;

					while (MatchesAt(i, j, newBoard[pieceToUse]) && maxIterations < 100)
                    {
						pieceToUse = Random.Range(0, newBoard.Count);
                        maxIterations++;
                    }
					//Make a container for the piece
                    Dot piece = newBoard[pieceToUse].GetComponent<Dot>();
                    maxIterations = 0;
					piece.column = i;
                    //Assign the row to the piece
					piece.row = j;
                    //Fill in the dots array with this new piece
					allDots[i, j] = newBoard[pieceToUse];
                    //Remove it from the list
					newBoard.Remove(newBoard[pieceToUse]);
				}
			}
		}
        //Check if it's still deadlocked
		if(IsDeadlocked())
		{
			StartCoroutine(ShuffleBoard());
		}
	}

}
