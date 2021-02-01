<form method='post' action='/avatar/save-avatar' enctype="multipart/form-data">
    <input type="text" name="hidrogenianId" id="hidrogenianId" />
    <input type="file" name="image" id="image" />
    <input type="submit" value="submit" />
</form>

<?php
echo json_encode($image).'<br/>';
echo json_encode($message).'<br/>';
?>