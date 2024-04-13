This utility is intended to be used in tandem with the nanDECK card generator for the LOTR-TCG.  It takes spreadsheets formatted for that program and turns each card row into three files:

* An hjson card definition stub for Gemp
* A Java unit test stub
* A wiki data struct

To run this, clone the repository locally and open it in Visual Studio.  Alter the contents of sheets.txt to contain one or more Google sheet IDs, one per line.  Then run the program to get the output.

The hjson files can be directly copy-pasted into an appropriate folder within the Gemp repo.  The unit test files can be copied into the appropriate folder structure under `gemp-lotr-server` and then IntelliJ will take care of the rest.

The unit tests have to have the `@Test` lines uncommented over each function; this is done so that they can be imported without introducing ten thousand failing tests.

Note that the CSV output of each sheet will be cached after one run, so if you alter the source sheet you will need to manually delete the cache under the bin folder.  

(Apologies for this and other weirdnesses; this hasn't been cleaned up for public consumption.) 

The templates of each file are stored as supremely hacky string literals in `GempCardImporter.cs`.  There are also a pair of flags set in that file, `Errata` and `Playtest`.  Set either of these to true to have the Gemp IDs generated for the resulting hjson cards automatically adjusted to avoid trampling over existing IDs.