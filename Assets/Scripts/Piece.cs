using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piece : MonoBehaviour
{
    public bool isWhite;
    public bool isKing;
    /// <summary>
    /// Sprawdzanie czy figura musi wykonać ruch
    /// </summary>
    /// <param name="p">tablica figur</param>
    /// <param name="x">położenie figury w poziomie></param>
    /// <param name="y">położenie figury w pionie></param>
    public bool IsForceToMove(Piece[,] board, int x, int y)
    {
        if (isWhite || isKing)
        {
            //Góra lewo
            if (x >= 2 && y <= 5)
            {
                Piece p = board[x - 1, y + 1];
                //Jeżeli figura jest innego koloru niż nasza
                if (p != null && p.isWhite != isWhite)
                {
                    //Sprawdź czy można wylądować za figurą przy zbiciu
                    if (board[x - 2, y + 2] == null)
                        return true;
                }
            }

            // Góra prawo
            if (x <= 5 && y <= 5)
            {
                Piece p = board[x + 1, y + 1];
                //Jeżeli figura jest innego koloru niż nasza
                if (p != null && p.isWhite != isWhite)
                {
                    //Sprawdź czy można wylądować za figurą przy zbiciu
                    if (board[x + 2, y + 2] == null)
                        return true;
                }
            }
        }
        
        if (!isWhite || isKing)
        {
                //Dół lewo
                if (x >= 2 && y >= 2)
                {
                    Piece p = board[x - 1, y - 1];
                //Jeżeli figura jest innego koloru niż nasza
                if (p != null && p.isWhite != isWhite)
                    {
                    //Sprawdź czy można wylądować za figurą przy zbiciu
                    if (board[x - 2, y - 2] == null)
                            return true;
                    }
                }

                // Dół prawo
                if (x <= 5 && y >= 2)
                {
                    Piece p = board[x + 1, y - 1];
                //Jeżeli figura jest innego koloru niż nasza
                if (p != null && p.isWhite != isWhite)
                    {
                    //Sprawdź czy można wylądować za figurą przy zbiciu
                    if (board[x + 2, y - 2] == null)
                            return true;
                    }
                }
            
        }
        return false;
    }
    /// <summary>
    /// Sprawdzanie czy ruch jest dozwolony
    /// </summary>
    /// <param name="board">tablica figur</param>
    /// <param name="x1">położenie początkowe figury w poziomie</param>
    /// <param name="y1">położenie początkowe figury w pionie</param>
    /// <param name="x2">położenie końcowe figury w poziomie</param>
    /// <param name="y2">położenie końcowe figury w pionie</param>
  public bool ValidMove(Piece[,] board, int x1, int y1, int x2, int y2)
    {
        //Jeżeli próbujesz przemieścić się na inną figurę
        if (board[x2, y2] != null)
            return false;

        int deltaMove = Mathf.Abs(x1 - x2);
        int deltaMoveY = y2 - y1;

        if(isWhite || isKing)
        {
            if (deltaMove == 1)
            {
                if (deltaMoveY == 1)
                    return true;
            }
            else if(deltaMove == 2)
            {
                if (deltaMoveY == 2)
                {
                    Piece p = board[(x1 + x2) / 2, (y1 + y2) / 2];
                    if (p != null && p.isWhite != isWhite)
                        return true;
                }
            }
        }
        if (!isWhite || isKing)
        {
            if (deltaMove == 1)
            {
                if (deltaMoveY == -1)
                    return true;
            }
            else if (deltaMove == 2)
            {
                if (deltaMoveY == -2)
                {
                    Piece p = board[(x1 + x2) / 2, (y1 + y2) / 2];
                    if (p != null && p.isWhite != isWhite)
                        return true;
                }
            }
        }
        return false;
    }
}
