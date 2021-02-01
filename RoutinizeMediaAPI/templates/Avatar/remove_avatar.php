<form method='delete' action='/avatar/remove-avatar'>
    <input type="text" name="image" id="image" />
    <input type="submit" value="submit" />
</form>

<?php
echo json_encode($message).'<br/>';
echo json_encode($imageName).'<br/>';
?>