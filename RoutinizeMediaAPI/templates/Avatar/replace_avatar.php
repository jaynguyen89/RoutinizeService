<form method='post' action='/avatar/replace-avatar' enctype="multipart/form-data">
    <input type="text" name="hidrogenianId" id="hidrogenianId" />
    <input type="text" name="current" id="current" />
    <input type="file" name="replaceBy" id="replaceBy" />
    <input type="submit" value="submit" />
</form>

<?php
echo json_encode($currentAvatar).'<br/>';
echo json_encode($message).'<br/>';
echo json_encode($newAvatar).'<br/>';
?>