# Menees.Diffs

This library contains non-visual, GUI-agnostic classes to find differences between text sequences, binary sequences, and directories. The code requires Menees.Common.

Text differencing is performed using the algorithm from [An O(ND) Difference Algorithm and Its Variations](http://www.menees.com/Docs/Diff/MyersDiff.pdf) by Eugene W. Myers.

Binary differencing is performed using the algorithm from:
* [A Linear Time, Constant Space Differencing Algorithm](http://www.menees.com/Docs/Diff/BurnsBinaryDiff_Paper.pdf) by Randal C. Burns and Darrell D. E. Long 
* [Differential Compression: A Generalized Solution For Binary Files](http://www.menees.com/Docs/Diff/BurnsBinaryDiff_Thesis.pdf) by Randal C. Burns
  * [Corrections to Figure 2.4 from page 21](http://www.menees.com/Docs/Diff/BurnsBinaryDiff_Fig24Fixed.gif)

The output of a binary diff can also be written as a [GDIFF](https://www.w3.org/TR/NOTE-gdiff-19970825.html) stream. 
