//Mitch Aufiero & Jordan Hicks
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Spawner : MonoBehaviour {

	enum State{ createMaze, cameraMove ,Play, Finish}
	State curState = State.createMaze;

	struct Cell
	{
		public GameObject northWall;
		public GameObject eastWall;

		public Cell(GameObject north, GameObject east)
		{
			northWall = north;
			eastWall = east;
		}
	}

	public Camera myCamera;
	public Camera playerCam;
	public int SIZE = 10;
	public GUIText timer;

    public GameObject[] rewards;
    public GameObject[] hinderance;
    public GameObject player;
	public GameObject horizontalWall;
	public GameObject verticalWall;
	public GameObject post;
	public GameObject finishLine;


	Cell[,] myCells;
	Vector3 startPosition;
	Vector3 endPosition;
	Cell entrance;
	Cell exit;

	private float startTime;
	private float speed = 50.0f;
	private float distance;
	private bool mapShown = false;// true to show map



	//for maze generation
	System.Random rand = new System.Random ();//to choose starting cell
	Stack<Vector2> maze = new Stack<Vector2> ();//stack of vector2s; used to store locations i and j
	int starti;
	int startj;
	List<Vector2> neighbors= new List<Vector2>();//list for neighbors of current cell
	
	Vector2 currentLocation ;
	Vector2 neighborLocation = new Vector2 (0, 0);


	bool testStartTime = true;
	// Use this for initialization
	void Start () {

		starti = rand.Next (1, SIZE);
		startj = rand.Next (1, SIZE);
		currentLocation = new Vector2 (starti, startj);// sets starting location for maze generation
		maze.Push (currentLocation);//pushes starting location on stack

		transform.localScale = new Vector3(SIZE * 2 , 0, SIZE * 2);//sets size of plane
		transform.position = transform.localScale * 5;

		myCamera.transform.position = new Vector3 (SIZE*5, SIZE*10, SIZE*5);//positions camera above maze
		startPosition = myCamera.transform.position;
		createGrid ();

		distance = Vector3.Distance (startPosition, endPosition);
		playerCam.GetComponent<Camera>().enabled = false;
		player.SetActive (false);



	}

	

	
	// Update is called once per frame
	void Update () {
		if (curState == State.createMaze) 
		{
			if (maze.Count > 0) // loops through until stack is empty
			{
				currentLocation =  maze.Peek ();
				int currentI = (int)currentLocation.x;
				int currentJ = (int)currentLocation.y;
				
				for (int row = -1; row < 2; row ++) // creates list of neighbors
				{	
					if (!(row == 0)) 
					{
						if(!(currentI+row > SIZE) && !(currentI +row < 1))
							if (checkCellUnvisited (currentI + row, currentJ))
								neighbors.Add (new Vector2((float)currentI + row, (float)currentJ));
						if(!(currentJ+row > SIZE) && !(currentJ +row < 1))
							if (checkCellUnvisited (currentI, currentJ + row))
								neighbors.Add( new Vector2((float)currentI, (float)currentJ+ row));
						
						
					}	
				}
				
				if(neighbors.Count == 0)//if all neighbors have been visited then pop current cell
				{
					maze.Pop();
					
				}
				else//if at least one neighbor has been visited choose neighbor and destroy wall
				{
					neighborLocation = neighbors [rand.Next (0, neighbors.Count)]; 
					maze.Push(neighborLocation);//puts random unvisited neighbor on the stack
					neighbors.Clear();
					
					
					if ((int)neighborLocation.x == currentI && (int)neighborLocation.y == currentJ - 1)
						destroyWall ((int)neighborLocation.x, (int)neighborLocation.y, true);// destroy new cell EastWall
					else if ((int)neighborLocation.x == currentI -1 && (int)neighborLocation.y == currentJ)
						destroyWall ((int)neighborLocation.x, (int)neighborLocation.y, false);// destroy new cell NorthWall	
					else if ((int)neighborLocation.x == currentI  && (int)neighborLocation.y == currentJ+1)
						destroyWall (currentI, currentJ, true);// destroy old cell East Wall	
					else if ((int)neighborLocation.x == currentI  +1&& (int)neighborLocation.y == currentJ)
						destroyWall (currentI, currentJ, false);// destroy old cell North Wall
				}
				
				
				if(maze.Count == 0)
					curState = State.cameraMove;
			}
			else
			{

				curState = State.cameraMove;

			}

		}

		if (curState == State.cameraMove) 
		{
			if(testStartTime)
			{
				createPath ();
				startTime = Time.time;
				testStartTime = false;
			}
			float distCovered = (Time.time - startTime) * speed;//how far the camera should be at time.time
			float fracJourney = distCovered / distance;
			myCamera.transform.position = Vector3.Lerp(startPosition, endPosition, fracJourney);//moves camera
			myCamera.transform.LookAt(transform);//camera Looks at plane
			if(myCamera.transform.position == endPosition)
			{	
				curState = State.Play;
				player.SetActive(true);
				player.transform.position = new Vector3(endPosition.x +2, 1.06f, endPosition.z);

				player.transform.rotation = myCamera.transform.rotation;


				myCamera.GetComponent<Camera>().enabled = false;

				playerCam.GetComponent<Camera>().enabled = true;
				startTime = 0;
			}
		}
		if (curState == State.Play) 
		{
						startTime += Time.deltaTime;
						timer.text = startTime.ToString ();

						
						if (Input.GetKeyDown(KeyCode.N) && !mapShown){
								showMap ();
								mapShown = true;
						}

						else if (Input.GetKeyDown(KeyCode.N) && mapShown) {
								closeMap ();
								mapShown = false;
						}
		}
	}
	
	void createGrid()
	{
		myCells = new Cell [SIZE+1, SIZE+1];
		GameObject myNorthWall = horizontalWall;
		GameObject myEastWall = verticalWall;

		for (int i = 0; i < SIZE+1; i ++) 
		{
			for (int j = 0; j < SIZE+1; j ++) 
			{

                // sets post size
				post.transform.position= new Vector3( (i* 10) , 2, (j*10));
				Instantiate(post, post.transform.position, post.transform.rotation);

				if(i > 0 )// creates horizontal walls except for when i = 0
				{
					horizontalWall.transform.position= new Vector3( (i* 10) - 5, 2, (j*10));
					myNorthWall = Instantiate(horizontalWall, horizontalWall.transform.position, horizontalWall.transform.rotation)as GameObject;

				}
				if( j > 0)// creates vertical walls except for when j = 0
				{
					verticalWall.transform.position= new Vector3((i * 10) , 2, (j * 10)-5);
					myEastWall = Instantiate(verticalWall, verticalWall.transform.position, verticalWall.transform.rotation)as GameObject;

				}

				myCells[i,j] = new Cell(myNorthWall, myEastWall);


			}
		}

	}//end createGrid()


	Vector3 getNorthWall(int i, int j)
	{
		return myCells[i,j].northWall.transform.position;
	}

	Vector3 getEastWall(int i, int j)//returns eastwall
	{
		return myCells[i,j].eastWall.transform.position;
	}

	Vector3 getNorthEastPost(int i , int j)//returns position of NorthEastPost of Cell[i,j]
	{
		return new Vector3 (getNorthWall (i, j).x + 10f, 2f, getEastWall (i, j).z + 10f);
	}

	bool checkCellUnvisited(int i, int j)//true = unvisited
	{
		bool walls = true;

		if (myCells [i, j].eastWall == null)
			walls = false;
		if (myCells [i, j].northWall == null)
			walls = false;
		if (myCells [i , j-1].northWall == null) 
			walls = false;
		if (myCells [i -1 , j].eastWall == null)
			walls = false;
			
			
			return walls;
	}


	void destroyWall(int i, int j, bool wall)// true for north; false for east
	{
		if (wall) 
		{
			Destroy (myCells[i,j].northWall);
			myCells[i,j].northWall = null;

		} else 
		{
			Destroy (myCells[i,j].eastWall);
			myCells[i,j].eastWall = null;
		}
	


	}

	void killTimer()
	{
		curState = State.Finish;
	}

	void createPath()
	{
		System.Random rand = new System.Random ();
		int ent = rand.Next (1, SIZE);
		entrance = myCells[1, ent];
		endPosition = myCells [0, ent].eastWall.transform.position;
		destroyWall (0, ent, false);
		ent = rand.Next (1, SIZE); 
		exit = myCells [SIZE, ent];
		finishLine.transform.position = myCells [SIZE, ent].eastWall.transform.position;
		destroyWall (SIZE, ent, false);

	}

	void showMap()
	{
		myCamera.GetComponent<Camera>().enabled = true;
		myCamera.GetComponent<Camera>().depth = 1;
		myCamera.transform.position = new Vector3 (SIZE*10, SIZE*20, SIZE*10);//positions camera above maze
		myCamera.transform.LookAt (transform);
		myCamera.GetComponent<Camera>().rect =  new Rect (.7f, .5f, .23f, .47f);
		myCamera.GetComponent<Camera>().cullingMask = 1 << 0;

	}
	void closeMap()
	{
		myCamera.GetComponent<Camera>().enabled = false;
	}
}
