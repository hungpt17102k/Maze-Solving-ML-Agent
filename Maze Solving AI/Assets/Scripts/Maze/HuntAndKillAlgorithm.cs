using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HuntAndKillAlgorithm : MazeAlgorithm
{
    private int currentRow = 0;
    private int currentColumn = 0;

    private bool mazeComplete = false;

    public HuntAndKillAlgorithm(MazeCell[,] mazeCells) : base(mazeCells)
    {
    }

    public override void CreateMaze()
    {
        HuntAndKill();
    }

    private void HuntAndKill() {
        mazeCells[currentRow, currentColumn].visited = true;

        while (!mazeComplete) {
            Kill(); // Will run until it hits a dead end.
            Hunt(); // Finds the next unvisited cell with an adjacent visited cell.
        }
    }

    private void Kill() {
        while (RouteStillAvailable(currentRow, currentColumn)) {
            int direction = ProceduralNumberGenerator.GetNextNumber();

            // Check the North side
            if (direction == 1 && CellIsAvailable(currentRow - 1, currentColumn)) {
                DestroyWallIfItExist(mazeCells[currentRow, currentColumn].northWall);
                DestroyWallIfItExist(mazeCells[currentRow - 1, currentColumn].southWall);
                currentRow--;
            }
            // Check the South side
            else if(direction == 2 && CellIsAvailable(currentRow + 1, currentColumn)) {
                DestroyWallIfItExist(mazeCells[currentRow, currentColumn].southWall);
                DestroyWallIfItExist(mazeCells[currentRow + 1, currentColumn].northWall);
                currentRow++;
            }
            // Check the East side
            else if(direction == 3 && CellIsAvailable(currentRow, currentColumn + 1)) {
                DestroyWallIfItExist(mazeCells[currentRow, currentColumn].eastWall);
                DestroyWallIfItExist(mazeCells[currentRow, currentColumn + 1].westWall);
                currentColumn++;
            }
            // Check the West side
            else if(direction == 4 && CellIsAvailable(currentRow, currentColumn - 1)) {
                DestroyWallIfItExist(mazeCells[currentRow, currentColumn].westWall);
                DestroyWallIfItExist(mazeCells[currentRow, currentColumn - 1].eastWall);
                currentColumn--;
            }

            mazeCells[currentRow, currentColumn].visited = true;
        }
    }

    private void Hunt() {
        mazeComplete = true; // Set it to this, and see if we prove otherwise below

        for(int r = 0; r < mazeRows; r++) {
            for(int c = 0; c < mazeColumns; c++) {
                if(!mazeCells[r,c].visited && CellHasAnAdjacentVisitedCell(r,c)) {
                    mazeComplete = false; // Yep, we found something so do another Kill cycle
                    currentRow = r;
                    currentColumn = c;
                    DestroyAdjacentWall(currentRow, currentColumn);
                    mazeCells[currentRow,currentColumn].visited = true;
                    return; // Exit the function
                }
            }
        }
    }

    private bool RouteStillAvailable(int row, int column) {
        int availableRoutes = 0;

        if(row > 0 && !mazeCells[row - 1,column].visited) {
            availableRoutes++;
        }

        if(row < mazeRows - 1 && !mazeCells[row + 1, column].visited) {
            availableRoutes++;
        }

        if (column > 0 && !mazeCells[row,column - 1].visited) {
            availableRoutes++;
        }

        if(column < mazeColumns - 1 && !mazeCells[row,column + 1].visited) {
            availableRoutes++;
        }

        return availableRoutes > 0;
    }

    private bool CellIsAvailable(int row, int column) {
        if(row >= 0 && row < mazeRows 
            && column >= 0 && column < mazeColumns 
            && !mazeCells[row, column].visited) 
        {
            return true;
        }
        else {
            return false;
        }
    }

    private void DestroyWallIfItExist(GameObject wall) {
        if(wall != null) {
            GameObject.Destroy(wall);
        }
    }

    private bool CellHasAnAdjacentVisitedCell(int row, int column) {
        int visitedCells = 0;

        // Look 1 row up (north) if we're on row 1 or greater
        if(row > 0 && mazeCells[row-1, column].visited) {
            visitedCells++;
        }

        // Look 1 row down (south) if we're on the second-to-last row or less
        if(row < (mazeRows - 2) && mazeCells[row + 1, column].visited) {
            visitedCells++;
        }

        // Look 1 column left (west) if we're on column 1 or greater
        if(column > 0 && mazeCells[row, column - 1].visited) {
            visitedCells++;
        }

        // Look 1 column left (east) if we're on the second-to-last row or less
        if(column < (mazeColumns - 2) && mazeCells[row, column + 1].visited) {
            visitedCells++;
        }

        // Return true if there are any adjacent visited cells to this one
        return visitedCells > 0;
    }

    private void DestroyAdjacentWall(int row, int column) {
        bool wallDestroyed = false;

        while(!wallDestroyed) {
            int direction = ProceduralNumberGenerator.GetNextNumber();

            if(direction == 1 && row > 0 && mazeCells[row - 1, column].visited) {
                DestroyWallIfItExist(mazeCells[row,column].northWall);
                DestroyWallIfItExist(mazeCells[row - 1,column].southWall);
                wallDestroyed = true;
            }
            else if(direction == 2 && row < (mazeRows - 2) && mazeCells[row + 1, column].visited) {
                DestroyWallIfItExist(mazeCells[row,column].southWall);
                DestroyWallIfItExist(mazeCells[row + 1,column].northWall);
                wallDestroyed = true;
            }
            else if(direction == 3 && column > 0 && mazeCells[row, column - 1].visited) {
                DestroyWallIfItExist(mazeCells[row,column].westWall);
                DestroyWallIfItExist(mazeCells[row,column - 1].eastWall);
                wallDestroyed = true;
            }
            else if(direction == 4 && column < (mazeColumns - 2) && mazeCells[row, column + 1].visited) {
                DestroyWallIfItExist(mazeCells[row,column].eastWall);
                DestroyWallIfItExist(mazeCells[row,column + 1].westWall);
                wallDestroyed = true;
            }
        }
    }
}
