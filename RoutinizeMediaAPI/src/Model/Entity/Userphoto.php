<?php
declare(strict_types=1);

namespace App\Model\Entity;

use Cake\ORM\Entity;

/**
 * Userphoto Entity
 *
 * @property int $Id
 * @property int|null $PhotoId
 * @property int|null $AccountId
 * @property bool|null $IsAvatar
 * @property bool|null $IsCover
 */
class Userphoto extends Entity
{
    /**
     * Fields that can be mass assigned using newEntity() or patchEntity().
     *
     * Note that when '*' is set to true, this allows all unspecified fields to
     * be mass assigned. For security purposes, it is advised to set '*' to false
     * (or remove it), and explicitly make individual fields accessible as needed.
     *
     * @var array
     */
    protected $_accessible = [
        'PhotoId' => true,
        'AccountId' => true,
        'IsAvatar' => true,
        'IsCover' => true,
    ];
}
