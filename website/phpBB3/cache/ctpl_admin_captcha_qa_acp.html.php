<?php if (!defined('IN_PHPBB')) exit; $this->_tpl_include('overall_header.html'); ?>


<a name="maincontent"></a>


	<a href="<?php if ($this->_rootref['U_LIST']) {  echo (isset($this->_rootref['U_LIST'])) ? $this->_rootref['U_LIST'] : ''; } else { echo (isset($this->_rootref['U_ACTION'])) ? $this->_rootref['U_ACTION'] : ''; } ?>" style="float: <?php echo (isset($this->_rootref['S_CONTENT_FLOW_END'])) ? $this->_rootref['S_CONTENT_FLOW_END'] : ''; ?>;">&laquo; <?php echo ((isset($this->_rootref['L_BACK'])) ? $this->_rootref['L_BACK'] : ((isset($user->lang['BACK'])) ? $user->lang['BACK'] : '{ BACK }')); ?></a>

	<h1><?php echo ((isset($this->_rootref['L_QUESTIONS'])) ? $this->_rootref['L_QUESTIONS'] : ((isset($user->lang['QUESTIONS'])) ? $user->lang['QUESTIONS'] : '{ QUESTIONS }')); ?></h1>

	<p><?php echo ((isset($this->_rootref['L_QUESTIONS_EXPLAIN'])) ? $this->_rootref['L_QUESTIONS_EXPLAIN'] : ((isset($user->lang['QUESTIONS_EXPLAIN'])) ? $user->lang['QUESTIONS_EXPLAIN'] : '{ QUESTIONS_EXPLAIN }')); ?></p>
<?php if ($this->_rootref['S_LIST']) {  ?>

	<form id="captcha_qa" method="post" action="<?php echo (isset($this->_rootref['U_ACTION'])) ? $this->_rootref['U_ACTION'] : ''; ?>">

	<fieldset class="tabulated">
	<legend><?php echo ((isset($this->_rootref['L_QUESTIONS'])) ? $this->_rootref['L_QUESTIONS'] : ((isset($user->lang['QUESTIONS'])) ? $user->lang['QUESTIONS'] : '{ QUESTIONS }')); ?></legend>

	<table cellspacing="1">
	<thead>
	<tr>
		<th colspan="3"><?php echo ((isset($this->_rootref['L_QUESTIONS'])) ? $this->_rootref['L_QUESTIONS'] : ((isset($user->lang['QUESTIONS'])) ? $user->lang['QUESTIONS'] : '{ QUESTIONS }')); ?></th>
	</tr>
	<tr class="row3">
		<td style="text-align: center;"><?php echo ((isset($this->_rootref['L_QUESTION_TEXT'])) ? $this->_rootref['L_QUESTION_TEXT'] : ((isset($user->lang['QUESTION_TEXT'])) ? $user->lang['QUESTION_TEXT'] : '{ QUESTION_TEXT }')); ?></td>
		<td style="width: 5%; text-align: center;"><?php echo ((isset($this->_rootref['L_QUESTION_LANG'])) ? $this->_rootref['L_QUESTION_LANG'] : ((isset($user->lang['QUESTION_LANG'])) ? $user->lang['QUESTION_LANG'] : '{ QUESTION_LANG }')); ?></td>
		<td style="vertical-align: top; width: 50px; text-align: center; white-space: nowrap;"><?php echo ((isset($this->_rootref['L_ACTION'])) ? $this->_rootref['L_ACTION'] : ((isset($user->lang['ACTION'])) ? $user->lang['ACTION'] : '{ ACTION }')); ?></td>
	</tr>
	</thead>
	<tbody>
	<?php $_questions_count = (isset($this->_tpldata['questions'])) ? sizeof($this->_tpldata['questions']) : 0;if ($_questions_count) {for ($_questions_i = 0; $_questions_i < $_questions_count; ++$_questions_i){$_questions_val = &$this->_tpldata['questions'][$_questions_i]; if (!($_questions_val['S_ROW_COUNT'] & 1)  ) {  ?><tr class="row1"><?php } else { ?><tr class="row2"><?php } ?>


		<td style="text-align: left;"><?php echo $_questions_val['QUESTION_TEXT']; ?></td>
		<td style="text-align: center;"><?php echo $_questions_val['QUESTION_LANG']; ?></td>
		<td style="text-align: center;"><a href="<?php echo $_questions_val['U_EDIT']; ?>"><?php echo (isset($this->_rootref['ICON_EDIT'])) ? $this->_rootref['ICON_EDIT'] : ''; ?></a>&nbsp;<a href="<?php echo $_questions_val['U_DELETE']; ?>"><?php echo (isset($this->_rootref['ICON_DELETE'])) ? $this->_rootref['ICON_DELETE'] : ''; ?></a></td>
		</tr>
		<?php }} ?>

	</tbody>
	</table>
	<fieldset class="quick">
		<input class="button1" type="submit" name="add" value="<?php echo ((isset($this->_rootref['L_ADD'])) ? $this->_rootref['L_ADD'] : ((isset($user->lang['ADD'])) ? $user->lang['ADD'] : '{ ADD }')); ?>" />
		<input  type="hidden" name="action" value="add" />
		<input  type="hidden" name="configure" value="1" />
		<input  type="hidden" name="select_captcha" value="<?php echo (isset($this->_rootref['CLASS'])) ? $this->_rootref['CLASS'] : ''; ?>" />

		<?php echo (isset($this->_rootref['S_FORM_TOKEN'])) ? $this->_rootref['S_FORM_TOKEN'] : ''; ?>

	</fieldset>
	<?php echo (isset($this->_rootref['S_FORM_TOKEN'])) ? $this->_rootref['S_FORM_TOKEN'] : ''; ?>

	</fieldset>
	</form>
<?php } else { if ($this->_rootref['S_ERROR']) {  ?>

		<div class="errorbox">
			<h3><?php echo ((isset($this->_rootref['L_WARNING'])) ? $this->_rootref['L_WARNING'] : ((isset($user->lang['WARNING'])) ? $user->lang['WARNING'] : '{ WARNING }')); ?></h3>
			<p><?php echo ((isset($this->_rootref['L_QA_ERROR_MSG'])) ? $this->_rootref['L_QA_ERROR_MSG'] : ((isset($user->lang['QA_ERROR_MSG'])) ? $user->lang['QA_ERROR_MSG'] : '{ QA_ERROR_MSG }')); ?></p>
		</div>
	<?php } ?>

	<form id="captcha_qa" method="post" action="<?php echo (isset($this->_rootref['U_ACTION'])) ? $this->_rootref['U_ACTION'] : ''; ?>">
	<fieldset>
		<legend><?php echo ((isset($this->_rootref['L_EDIT_QUESTION'])) ? $this->_rootref['L_EDIT_QUESTION'] : ((isset($user->lang['EDIT_QUESTION'])) ? $user->lang['EDIT_QUESTION'] : '{ EDIT_QUESTION }')); ?></legend>
	<dl>
		<dt><label for="strict"><?php echo ((isset($this->_rootref['L_QUESTION_STRICT'])) ? $this->_rootref['L_QUESTION_STRICT'] : ((isset($user->lang['QUESTION_STRICT'])) ? $user->lang['QUESTION_STRICT'] : '{ QUESTION_STRICT }')); ?>:</label><br /><span><?php echo ((isset($this->_rootref['L_QUESTION_STRICT_EXPLAIN'])) ? $this->_rootref['L_QUESTION_STRICT_EXPLAIN'] : ((isset($user->lang['QUESTION_STRICT_EXPLAIN'])) ? $user->lang['QUESTION_STRICT_EXPLAIN'] : '{ QUESTION_STRICT_EXPLAIN }')); ?></span></dt>
		<dd><label><input type="radio" class="radio" name="strict" value="1"<?php if ($this->_rootref['STRICT']) {  ?> id="strict" checked="checked"<?php } ?> /> <?php echo ((isset($this->_rootref['L_YES'])) ? $this->_rootref['L_YES'] : ((isset($user->lang['YES'])) ? $user->lang['YES'] : '{ YES }')); ?></label>
			<label><input type="radio" class="radio" name="strict" value="0"<?php if (! $this->_rootref['STRICT']) {  ?> id="strict" checked="checked"<?php } ?> /> <?php echo ((isset($this->_rootref['L_NO'])) ? $this->_rootref['L_NO'] : ((isset($user->lang['NO'])) ? $user->lang['NO'] : '{ NO }')); ?></label></dd>
	</dl>

	<dl>
		<dt><label for="lang_iso"><?php echo ((isset($this->_rootref['L_QUESTION_LANG'])) ? $this->_rootref['L_QUESTION_LANG'] : ((isset($user->lang['QUESTION_LANG'])) ? $user->lang['QUESTION_LANG'] : '{ QUESTION_LANG }')); ?></label><br /><span><?php echo ((isset($this->_rootref['L_QUESTION_LANG_EXPLAIN'])) ? $this->_rootref['L_QUESTION_LANG_EXPLAIN'] : ((isset($user->lang['QUESTION_LANG_EXPLAIN'])) ? $user->lang['QUESTION_LANG_EXPLAIN'] : '{ QUESTION_LANG_EXPLAIN }')); ?></span></dt>
		<dd><select id="lang_iso" name="lang_iso"><?php $_langs_count = (isset($this->_tpldata['langs'])) ? sizeof($this->_tpldata['langs']) : 0;if ($_langs_count) {for ($_langs_i = 0; $_langs_i < $_langs_count; ++$_langs_i){$_langs_val = &$this->_tpldata['langs'][$_langs_i]; ?><option value="<?php echo $_langs_val['ISO']; ?>" <?php if ($_langs_val['ISO'] == $this->_rootref['LANG_ISO']) {  ?> selected="selected" <?php } ?>><?php echo $_langs_val['NAME']; ?></option><?php }} ?></select></dd>
	</dl>
	<dl>
		<dt><label for="question_text"><?php echo ((isset($this->_rootref['L_QUESTION_TEXT'])) ? $this->_rootref['L_QUESTION_TEXT'] : ((isset($user->lang['QUESTION_TEXT'])) ? $user->lang['QUESTION_TEXT'] : '{ QUESTION_TEXT }')); ?></label><br /><span><?php echo ((isset($this->_rootref['L_QUESTION_TEXT_EXPLAIN'])) ? $this->_rootref['L_QUESTION_TEXT_EXPLAIN'] : ((isset($user->lang['QUESTION_TEXT_EXPLAIN'])) ? $user->lang['QUESTION_TEXT_EXPLAIN'] : '{ QUESTION_TEXT_EXPLAIN }')); ?></span></dt>
		<dd><input id="question_text" maxlength="255" size="60" name="question_text" type="text" value="<?php echo (isset($this->_rootref['QUESTION_TEXT'])) ? $this->_rootref['QUESTION_TEXT'] : ''; ?>" /></dd>
	</dl>
	<dl>
		<dt><label for="answers"><?php echo ((isset($this->_rootref['L_QUESTION_ANSWERS'])) ? $this->_rootref['L_QUESTION_ANSWERS'] : ((isset($user->lang['QUESTION_ANSWERS'])) ? $user->lang['QUESTION_ANSWERS'] : '{ QUESTION_ANSWERS }')); ?></label><br /><span><?php echo ((isset($this->_rootref['L_ANSWERS_EXPLAIN'])) ? $this->_rootref['L_ANSWERS_EXPLAIN'] : ((isset($user->lang['ANSWERS_EXPLAIN'])) ? $user->lang['ANSWERS_EXPLAIN'] : '{ ANSWERS_EXPLAIN }')); ?></span></dt>
		<dd><textarea id="answers" style="word-wrap: normal; overflow-x: scroll;" name="answers" rows="15" cols="800" ><?php echo (isset($this->_rootref['ANSWERS'])) ? $this->_rootref['ANSWERS'] : ''; ?></textarea></dd>
	</dl>
	</fieldset>
	<fieldset class="quick">
		<input class="button1" type="submit" name="submit" value="<?php echo ((isset($this->_rootref['L_SUBMIT'])) ? $this->_rootref['L_SUBMIT'] : ((isset($user->lang['SUBMIT'])) ? $user->lang['SUBMIT'] : '{ SUBMIT }')); ?>" />
		<input type="hidden" name="question_id" value="<?php echo (isset($this->_rootref['QUESTION_ID'])) ? $this->_rootref['QUESTION_ID'] : ''; ?>" />
		<input type="hidden" name="action" value="add" />
		<input  type="hidden" name="configure" value="1" />
		<input  type="hidden" name="select_captcha" value="<?php echo (isset($this->_rootref['CLASS'])) ? $this->_rootref['CLASS'] : ''; ?>" />

		<?php echo (isset($this->_rootref['S_FORM_TOKEN'])) ? $this->_rootref['S_FORM_TOKEN'] : ''; ?>

	</fieldset>
	</form>
<?php } $this->_tpl_include('overall_footer.html'); ?>