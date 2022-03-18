using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Maze : MonoBehaviour
{
    [Header("Material to build maze")]
    public GameObject wall;
    public GameObject floor;

    public int row = 4;
    public int column = 4;

    private MazeCell[,] grid;

    [Header("Start and finish objects")]
    public GameObject goal;

    public Transform insPos;


    // Start is called before the first frame update
    void Start()
    {
        // First create the grid with all the walls and floors
        CreateGrid();

        // Then run the algorithm to carve the path from top left to bottom right
        MazeAlgorithm ma = new HuntAndKillAlgorithm(grid);
        ma.CreateMaze();
    }

    void CreateGrid()
    {
        float floorPosY = floor.transform.localScale.y / 2;
        float size = floor.transform.localScale.x;
        // float wallPos_Adding = size / 2 - wall.transform.localScale.z / 2;
        float wallPosY = wall.transform.localScale.y / 2;

        grid = new MazeCell[row, column];

        for (int r = 0; r < row; r++)
        {
            for (int c = 0; c < column; c++)
            {
                grid[r,c] = new MazeCell();

                // Add floor
                grid[r,c].floor = Instantiate(floor, new Vector3(r * size + insPos.position.x, -floorPosY, c * size + insPos.position.z), Quaternion.identity);
                grid[r,c].floor.name = "Floor " + r + "_" + c;
                grid[r,c].floor.transform.parent = this.gameObject.transform.GetChild(0);
                
                // Add west wall side
                if(c == 0) {
                    grid[r,c].westWall = Instantiate(wall, new Vector3(r * size + insPos.position.x, wallPosY, c * size - size/2 + insPos.position.z), Quaternion.identity);
                    grid[r,c].westWall.name = "West Wall " + r + "_" + c;
                    grid[r,c].westWall.transform.parent = gameObject.transform.GetChild(1);
                }
                
                // Add east wall side
                grid[r,c].eastWall = Instantiate(wall, new Vector3(r * size + insPos.position.x, wallPosY, c * size + size/2 + insPos.position.z), Quaternion.identity);
                grid[r,c].eastWall.name = "East Wall " + r + "_" + c;
                grid[r,c].eastWall.transform.parent = gameObject.transform.GetChild(2); 

                // Add north wall side 
                if(r == 0) {
                    grid[r,c].northWall = Instantiate(wall, new Vector3(r*size - size/2 + insPos.position.x, wallPosY, c*size + insPos.position.z), Quaternion.Euler(0, 90,0));
                    grid[r,c].northWall.name = "North Wall " + r + "_" + c;
                    grid[r,c].northWall.transform.parent = gameObject.transform.GetChild(3); 
                }  

                // Add south wall side 
                grid[r,c].southWall = Instantiate(wall, new Vector3(r*size + size/2 + insPos.position.x, wallPosY, c*size + insPos.position.z), Quaternion.Euler(0, 90,0));
                grid[r,c].southWall.name = "South Wall " + r + "_" + c;
                grid[r,c].southWall.transform.parent = gameObject.transform.GetChild(4);   
            }
        }

        // Spawning goal 
        Vector3 goalPosXZ = grid[row - 1, column - 1].floor.transform.position;
        SpawnObject(goal, new Vector3(goalPosXZ.x, goal.transform.localScale.y / 2, goalPosXZ.z));
    }

    void SpawnObject(GameObject obj, Vector3 pos) {
        GameObject o = Instantiate(obj, pos, Quaternion.identity);
        o.transform.parent = this.transform;
    }

}
