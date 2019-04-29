<?php
	$text1 = $_POST["time"];
	$text2 = $_POST["log"];

	if ($text1 != "") {
		echo("Sent!");
		echo("Field 1:".$text1);
		echo("Field 2:".$text2);
		$file = fopen("log.txt", "a");
		fwrite($file, $text1);
		fwrite($file, $text2);
		fclose($file);
	}
	else {
		echo("Failed!");
	}
?>