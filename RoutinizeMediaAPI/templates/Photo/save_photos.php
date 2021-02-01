<form method='post' action='/photo/save-photos' enctype="multipart/form-data">
    <input type="text" name="hidrogenianId" id="hidrogenianId" />
    <input type="file" name="images[]" id="images" multiple /><br />
    <input type="submit" value="submit" />
</form>

<?php
echo json_encode($images).'<br/>';
echo json_encode($message).'<br/>';
?>
